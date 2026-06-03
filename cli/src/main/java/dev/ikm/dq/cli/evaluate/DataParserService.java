package dev.ikm.dq.cli.evaluate;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.node.ArrayNode;
import com.fasterxml.jackson.databind.node.ObjectNode;
import org.apache.commons.csv.CSVFormat;
import org.apache.commons.csv.CSVParser;
import org.apache.commons.csv.CSVRecord;
import org.springframework.stereotype.Service;

import java.io.IOException;
import java.io.Reader;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Iterator;
import java.util.Spliterator;
import java.util.Spliterators;
import java.util.stream.Stream;
import java.util.stream.StreamSupport;

@Service
public class DataParserService {

	private record MessageWrapper(String id, String message) {
	}

	public Stream<PiqiRequest> parseCSVData(Path data, RunContext runContext) throws IOException {
		Reader reader = Files.newBufferedReader(data);
		CSVFormat csvFormat = CSVFormat.DEFAULT
				.builder()
				.setHeader()
				.setSkipHeaderRecord(true)
				.get();
		CSVParser csvParser = csvFormat.parse(reader);

		Iterator<CSVRecord> it = csvParser.iterator();

		Spliterator<CSVRecord> spliterator =
				Spliterators.spliteratorUnknownSize(it, Spliterator.ORDERED | Spliterator.NONNULL);

		return StreamSupport.stream(spliterator, false)
				.map(csvRecord -> transformCSVToPiqiMessage(csvRecord, runContext))
				.map(messageWrapper ->
						new PiqiRequest(
								runContext.dataProviderId(),
								runContext.dataSourceId(),
								messageWrapper.id(),
								runContext.modelMnemonic(),
								runContext.rubricMnemonic(),
								messageWrapper.message()))
				.onClose(() -> {
					try {
						csvParser.close();
					} catch (IOException ignored) {
					}
					try {
						reader.close();
					} catch (IOException ignored) {
					}
				});
	}

	// Column mapping from index.html
	// 0: UniqueID
	// 3: LabChemTestName
	// 6: LabChemResultValue
	// 9: Topography
	// 11: AccessioningInstitution
	// 17: LOINC
	// 18: Units
	// 19: Abnormal
	// 20: RefHigh
	// 21: RefLow
	private MessageWrapper transformCSVToPiqiMessage(CSVRecord csvRecord, RunContext runContext) {
		final ObjectMapper objectMapper = new ObjectMapper();

		String uniqueId = val(csvRecord, 0);
		String testName = val(csvRecord, 3);
		String resultValue = val(csvRecord, 6);
		String topography = val(csvRecord, 9);
		String accessioningInstitution = val(csvRecord, 11);
		String loinc = val(csvRecord, 17);
		String units = val(csvRecord, 18);
		String abnormal = val(csvRecord, 19);
		String refHigh = val(csvRecord, 20);
		String refLow = val(csvRecord, 21);

		ObjectNode messageData = objectMapper.createObjectNode();
		messageData.put("messageId", uniqueId);
		messageData.put("formatID", "");
		messageData.put("useCaseID", "");

		ObjectNode patient = objectMapper.createObjectNode();
		patient.put("id", uniqueId);

		ArrayNode labResults = objectMapper.createArrayNode();
		ObjectNode labResult = objectMapper.createObjectNode();

		// test
		ObjectNode test = objectMapper.createObjectNode();
		ArrayNode testCodings = objectMapper.createArrayNode();
		if (!loinc.isEmpty()) {
			ObjectNode coding = objectMapper.createObjectNode();
			coding.put("code", loinc);
			coding.put("display", testName);
			coding.put("system", "2.16.840.1.113883.6.1");
			testCodings.add(coding);
		}
		test.set("codings", testCodings);
		test.put("text", testName);
		labResult.set("test", test);

		// performingSite
		ObjectNode performingSite = objectMapper.createObjectNode();
		performingSite.put("text", accessioningInstitution);
		labResult.set("performingSite", performingSite);

		// specimenType
		ObjectNode specimenType = objectMapper.createObjectNode();
		specimenType.put("text", topography);
		labResult.set("specimenType", specimenType);

		// abnormalFlag
		ObjectNode abnormalFlag = objectMapper.createObjectNode();
		ArrayNode abCodings = objectMapper.createArrayNode();
		if (!abnormal.isEmpty()) {
			ObjectNode ab = objectMapper.createObjectNode();
			ab.put("code", abnormal);
			ab.put("display", abnormal);
			ab.put("system", "AbFlag");
			abCodings.add(ab);
		}
		abnormalFlag.set("codings", abCodings);
		abnormalFlag.put("text", abnormal);
		labResult.set("abnormalFlag", abnormalFlag);

		// referenceRange
		ObjectNode referenceRange = objectMapper.createObjectNode();
		referenceRange.put("text", refLow + " - " + refHigh);
		if (!refLow.isEmpty()) referenceRange.put("lowValue", refLow);
		if (!refHigh.isEmpty()) referenceRange.put("highValue", refHigh);
		labResult.set("referenceRange", referenceRange);

		// resultValue
		ObjectNode resultValueNode = objectMapper.createObjectNode();
		ArrayNode rvCodings = objectMapper.createArrayNode();
		if (!resultValue.isEmpty()) {
			ObjectNode rv = objectMapper.createObjectNode();
			rv.put("code", resultValue);
			rv.put("display", resultValue);
			rv.put("system", "ResVal");
			rvCodings.add(rv);
		}
		resultValueNode.set("codings", rvCodings);
		resultValueNode.put("text", resultValue);
		labResult.set("resultValue", resultValueNode);

		// resultUnit
		ObjectNode resultUnit = objectMapper.createObjectNode();
		ArrayNode ruCodings = objectMapper.createArrayNode();
		if (!units.isEmpty()) {
			ObjectNode ru = objectMapper.createObjectNode();
			ru.put("code", units);
			ru.put("display", units);
			ru.put("system", "UCUM");
			ruCodings.add(ru);
		}
		resultUnit.set("codings", ruCodings);
		resultUnit.put("text", units);
		labResult.set("resultUnit", resultUnit);

		// interpretation
		ObjectNode interpretation = objectMapper.createObjectNode();
		ArrayNode intCodings = objectMapper.createArrayNode();
		ObjectNode intCode = objectMapper.createObjectNode();
		if (!abnormal.isEmpty()) {
			intCode.put("code", abnormal);
			intCode.put("system", "2.16.840.1.113883.5.83");
			interpretation.put("text", abnormal);
		} else {
			intCode.put("code", "N");
			intCode.put("system", "2.16.840.1.113883.5.83");
			interpretation.put("text", "N");
		}
		intCodings.add(intCode);
		interpretation.set("codings", intCodings);
		labResult.set("interpretation", interpretation);

		labResults.add(labResult);
		patient.set("labResults", labResults);
		messageData.set("patient", patient);

		messageData.put("dataSourceID", runContext.dataSourceId());
		messageData.put("dataProviderID", runContext.dataProviderId());
		messageData.put("messageID", uniqueId);

		return new MessageWrapper(uniqueId, messageData.toString());
	}

	private String val(CSVRecord r, int index) {
		return index < r.size() && r.get(index) != null ? r.get(index) : "";
	}
}
