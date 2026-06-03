// Path: cli/src/main/java/dev/ikm/dq/cli/evaluate/PiqiResponseDtos.java
package dev.ikm.dq.cli.evaluate;

import com.fasterxml.jackson.annotation.JsonAlias;
import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;
import com.fasterxml.jackson.databind.JsonNode;

import java.util.List;
import java.util.Map;

/**
 * Data Transfer Objects (DTOs) for deserializing the PIQI Engine's JSON response.
 * These records use Jackson annotations to map the JSON structure to strongly-typed Java objects.
 */
public final class PiqiResponseDtos {

	@JsonIgnoreProperties(ignoreUnknown = true)
	public record PiqiResponse(
			ScoringData scoringData,
			String auditedMessage
	) {
	}

	@JsonIgnoreProperties(ignoreUnknown = true)
	public record ScoringData(
			List<DataClassResult> dataClassResults
	) {
	}

	@JsonIgnoreProperties(ignoreUnknown = true)
	public record DataClassResult(
			String dataClassName,
			int instanceCount
	) {
	}

	@JsonIgnoreProperties(ignoreUnknown = true)
	public record AuditedMessage(
			@JsonProperty("MessageID")
			@JsonAlias("messageID") // Now accepts either casing
			String messageId,
			Patient patient
	) {
	}

	@JsonIgnoreProperties(ignoreUnknown = true)
	public record Patient(
			@JsonProperty("labResults") List<Map<String, Attribute>> labResults,
			@JsonProperty("medications") List<Map<String, Attribute>> medications,
			@JsonProperty("allergies") List<Map<String, Attribute>> allergies,
			@JsonProperty("conditions") List<Map<String, Attribute>> conditions,
			@JsonProperty("procedures") List<Map<String, Attribute>> procedures,
			@JsonProperty("vitalSigns") List<Map<String, Attribute>> vitalSigns,
			@JsonProperty("immunizations") List<Map<String, Attribute>> immunizations,
			@JsonProperty("demographics") List<Map<String, Attribute>> demographics,
			@JsonProperty("encounters") List<Map<String, Attribute>> encounters,
			@JsonProperty("providers") List<Map<String, Attribute>> providers,
			@JsonProperty("clinicalDocuments") List<Map<String, Attribute>> clinicalDocuments,
			@JsonProperty("diagnosticImaging") List<Map<String, Attribute>> diagnosticImaging,
			@JsonProperty("goals") List<Map<String, Attribute>> goals,
			@JsonProperty("healthAssessments") List<Map<String, Attribute>> healthAssessments,
			@JsonProperty("medicalDevices") List<Map<String, Attribute>> medicalDevices
	) {
	}

	@JsonIgnoreProperties(ignoreUnknown = true)
	public record Attribute(
			AttributeAudit attributeAudit,
			JsonNode data
	) {
	}

	@JsonIgnoreProperties(ignoreUnknown = true)
	public record AttributeAudit(
			List<AssessmentItem> assessmentItems
	) {
	}

	@JsonIgnoreProperties(ignoreUnknown = true)
	public record AssessmentItem(
			String attributeName,
			String assessment,
			String status,
			String reason,
			String effect
	) {
	}
}