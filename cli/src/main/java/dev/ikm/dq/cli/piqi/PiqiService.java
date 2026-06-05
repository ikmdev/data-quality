package dev.ikm.dq.cli.piqi;

import com.fasterxml.jackson.databind.ObjectMapper;
import dev.ikm.dq.cli.evaluate.EvaluationResult;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;
import org.springframework.web.client.RestClient;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Map;

import static dev.ikm.dq.cli.piqi.PiqiResponseDtos.*;

@Service
public class PiqiService {

    Logger LOG = LoggerFactory.getLogger(PiqiService.class);

    private final RestClient restClient;
    private final ObjectMapper objectMapper;

    @Value("${piqi.score-path}")
    private String piqiScorePath;

    // A map to convert data class names from the response to the keys used in the auditedMessage JSON
    private static final Map<String, String> DATA_CLASS_TO_PATH_MAP = Map.ofEntries(
            Map.entry("Lab Results", "labResults"),
            Map.entry("Medications", "medications"),
            Map.entry("Allergies", "allergies"),
            Map.entry("Conditions", "conditions"),
            Map.entry("Procedures", "procedures"),
            Map.entry("Vital Signs", "vitalSigns"),
            Map.entry("Immunizations", "immunizations"),
            Map.entry("Demographics", "demographics"),
            Map.entry("Encounters", "encounters"),
            Map.entry("Providers", "providers"),
            Map.entry("Clinical Documents", "clinicalDocuments"),
            Map.entry("Diagnostic Imaging", "diagnosticImaging"),
            Map.entry("Goals", "goals"),
            Map.entry("Health Assessments", "healthAssessments"),
            Map.entry("Medical Devices", "medicalDevices")
    );

    @Autowired
    public PiqiService(RestClient restClient, ObjectMapper objectMapper) {
        this.restClient = restClient;
        this.objectMapper = objectMapper;
    }

    /**
     * Sends a request to the PIQI Engine and extracts a list of evaluation results from the response.
     *
     * @param piqiRequest The request containing the message data to be evaluated.
     * @return A list of EvaluationResult objects parsed from the PIQI Engine's response.
     */
    public List<EvaluationResult> sendRequestToPiqiEngine(long runId, PiqiRequest piqiRequest) {
        String jsonBody = buildEscapedPayload(
                piqiRequest.dataProviderID(),
                piqiRequest.dataSourceID(),
                piqiRequest.messageID(),
                piqiRequest.messageData()
        );

        // 1. Call the PIQI Engine
        String responseJson = restClient.post()
                .uri(piqiScorePath)
                .body(jsonBody)
                .retrieve()
                .body(String.class);

        if (responseJson == null || responseJson.isEmpty()) {
            LOG.error("Received an empty response from PIQI Engine.");
            return Collections.emptyList();
        }

        // 2. Parse the response and extract results
        try {
            return extractEvaluationResults(responseJson, runId);
        } catch (IOException e) {
            System.err.println("Failed to parse PIQI Engine response: " + e.getMessage());
            e.printStackTrace();
            return Collections.emptyList();
        }
    }

    /**
     * Parses the JSON response from the PIQI engine to extract detailed evaluation results.
     *
     * @param responseJson The full JSON response string from the REST client.
     * @param runId The ID of the evaluation run, passed from the original request.
     * @return A list of EvaluationResult records.
     * @throws IOException if JSON parsing fails.
     */
    private List<EvaluationResult> extractEvaluationResults(String responseJson, Long runId) throws IOException {
        List<EvaluationResult> results = new ArrayList<>();

        // Step 1: Parse the main response which contains the escaped 'auditedMessage'
        PiqiResponse piqiResponse = objectMapper.readValue(responseJson, PiqiResponse.class);
        if (piqiResponse == null || piqiResponse.auditedMessage() == null || piqiResponse.scoringData() == null) {
            return Collections.emptyList();
        }

        // Step 2: Parse the inner 'auditedMessage' JSON string
        AuditedMessage auditedMessage = objectMapper.readValue(piqiResponse.auditedMessage(), AuditedMessage.class);
        if (auditedMessage == null || auditedMessage.patient() == null) {
            return Collections.emptyList();
        }
        String messageId = auditedMessage.messageId(); // Extracted from the inner JSON

        // Step 3: Iterate through the data classes that were actually assessed
        for (DataClassResult dataClassResult : piqiResponse.scoringData().dataClassResults()) {
            if (dataClassResult.instanceCount() <= 0) {
                continue; // Skip data classes that had no instances in the message
            }

            String dataPath = DATA_CLASS_TO_PATH_MAP.get(dataClassResult.dataClassName());
            if (dataPath == null) {
                continue; // Skip if we don't have a mapping for this data class
            }

            // Use reflection to get the list of elements (e.g., patient.labResults())
            List<Map<String, Attribute>> elements;
            try {
                elements = (List<Map<String, Attribute>>) Patient.class.getMethod(dataPath).invoke(auditedMessage.patient());
            } catch (Exception e) {
                // This can happen if the method name in the map doesn't match the record component name
                System.err.println("Could not find or invoke method for data path: " + dataPath);
                continue;
            }

            if (elements == null) continue;

            // Step 4: Traverse the nested structure to find the assessment items
            for (Map<String, Attribute> element : elements) {
                for (Attribute attribute : element.values()) {
                    if (attribute != null && attribute.attributeAudit() != null && attribute.attributeAudit().assessmentItems() != null) {
                        for (AssessmentItem item : attribute.attributeAudit().assessmentItems()) {
                            // For each assessment item, create an EvaluationResult
                            results.add(new EvaluationResult(
                                    runId,
                                    messageId,
                                    dataClassResult.dataClassName(),
                                    item.attributeName(),
                                    objectMapper.writeValueAsString(attribute.data()), // Serialize the 'data' object back to a string
                                    item.assessment(),
                                    item.status(),
                                    item.reason(),
                                    item.effect()
                            ));
                        }
                    }
                }
            }
        }
        return results;
    }

    /**
     * Builds the JSON payload string for the PIQI Engine request.
     * This manual approach is kept from the original implementation for performance.
     */
    public String buildEscapedPayload(String dataProviderID, String dataSourceID, String messageID, String innerJson) {
        String escapedInner = innerJson.replace("\\", "\\\\").replace("\"", "\\\"");

        StringBuilder sb = new StringBuilder(innerJson.length() + 500);
        sb.append("{ \"dataProviderID\": \"").append(dataProviderID).append("\", ");
        sb.append("\"dataSourceID\": \"").append(dataSourceID).append("\", ");
        sb.append("\"messageID\": \"").append(messageID).append("\", ");
        sb.append("\"piqiModelMnemonic\": \"PAT_CLINICAL_V1\", ");
        sb.append("\"evaluationRubricMnemonic\": \"Basic_VA_Lab\", ");
        sb.append("\"messageData\": \"").append(escapedInner).append("\" }");

        return sb.toString();
    }
}