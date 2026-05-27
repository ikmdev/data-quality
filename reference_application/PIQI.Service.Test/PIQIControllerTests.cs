using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using PIQI.Components.Models;
using PIQI.Service.WebTesting.Rest;
using System.Net;
using System.Text.RegularExpressions;

namespace PIQI.Service.Test;

public class PIQIControllerTests : RestClient, IClassFixture<WebApplicationFactory<PIQI_Engine.Server.Program>>
{
    private readonly HttpClient _client;
    public PIQIControllerTests(WebApplicationFactory<PIQI_Engine.Server.Program> factory) : base(factory.CreateClient())
    {
        var application = new PIQIEngineService();
        _client = application.CreateClient();
    }

    [Theory]
    [InlineData("/PIQI/ScoreMessage")]
    public async Task ScoresMessage1_ReturnsExpectedResponse(string endpoint)
    {
        // Arrange
        var piqiRequest = new PIQIRequest
        {
            DataProviderID = "TestProvider",
            DataSourceID = "TestSource",
            PIQIModelMnemonic = "PAT_CLINICAL_V1",
            EvaluationRubricMnemonic = "USCDI_V3",
            MessageID = "Msg001",
            MessageData = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData/Test1_PIQI.json"))
        };
        var result = new PIQIResponse();
        var requestContent = new StringContent(JsonConvert.SerializeObject(piqiRequest), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(endpoint, requestContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var contentType = response.Content.Headers.ContentType.MediaType;
        if (contentType == "text/plain" || contentType == "application/json")
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            result = JsonConvert.DeserializeObject<PIQIResponse>(responseBody);

            Assert.NotNull(result);
        }

        #region Check Results

        #region Overall Message Results
        Assert.Equal(result.Succeeded, true);
        Assert.Equal(result.ScoringData.MessageResults.PIQIScore, 54);
        Assert.Equal(result.ScoringData.MessageResults.Numerator, 6);
        Assert.Equal(result.ScoringData.MessageResults.Denominator, 11);
        Assert.Equal(result.ScoringData.MessageResults.CriticalFailureCount, 0);
        Assert.Equal(result.ScoringData.MessageResults.WeightedPIQIScore, 54);
        Assert.Equal(result.ScoringData.MessageResults.WeightedNumerator, 6);
        Assert.Equal(result.ScoringData.MessageResults.WeightedDenominator, 11);
        #endregion

        #region Data Class Results
        Assert.Equal(result.ScoringData.DataClassResults.Count, 15);

        #region Allergies
        var allergies = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Allergies");
        Assert.Equal(allergies.InstanceCount, 0);
        Assert.Equal(allergies.PIQIScore, 0);
        Assert.Equal(allergies.Numerator, 0);
        Assert.Equal(allergies.Denominator, 0);
        Assert.Equal(allergies.CriticalFailureCount, 0);
        Assert.Equal(allergies.WeightedPIQIScore, 0);
        Assert.Equal(allergies.WeightedNumerator, 0);
        Assert.Equal(allergies.WeightedDenominator, 0);
        #endregion

        #region Clinical Documents
        var clinicalDocuments = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Clinical Documents");
        Assert.Equal(clinicalDocuments.InstanceCount, 0);
        Assert.Equal(clinicalDocuments.PIQIScore, 0);
        Assert.Equal(clinicalDocuments.Numerator, 0);
        Assert.Equal(clinicalDocuments.Denominator, 0);
        Assert.Equal(clinicalDocuments.CriticalFailureCount, 0);
        Assert.Equal(clinicalDocuments.WeightedPIQIScore, 0);
        Assert.Equal(clinicalDocuments.WeightedNumerator, 0);
        Assert.Equal(clinicalDocuments.WeightedDenominator, 0);
        #endregion

        #region Conditions
        var conditions = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Conditions");
        Assert.Equal(conditions.InstanceCount, 0);
        Assert.Equal(conditions.PIQIScore, 0);
        Assert.Equal(conditions.Numerator, 0);
        Assert.Equal(conditions.Denominator, 0);
        Assert.Equal(conditions.CriticalFailureCount, 0);
        Assert.Equal(conditions.WeightedPIQIScore, 0);
        Assert.Equal(conditions.WeightedNumerator, 0);
        Assert.Equal(conditions.WeightedDenominator, 0);
        #endregion

        #region Demographics
        var demographics = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Demographics");
        Assert.Equal(demographics.InstanceCount, 1);
        Assert.Equal(demographics.PIQIScore, 75);
        Assert.Equal(demographics.Numerator, 3);
        Assert.Equal(demographics.Denominator, 4);
        Assert.Equal(demographics.CriticalFailureCount, 0);
        Assert.Equal(demographics.WeightedPIQIScore, 75);
        Assert.Equal(demographics.WeightedNumerator, 3);
        Assert.Equal(demographics.WeightedDenominator, 4);
        #endregion

        #region Diagnostic Imaging
        var diagnosticImaging = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Diagnostic Imaging");
        Assert.Equal(diagnosticImaging.InstanceCount, 0);
        Assert.Equal(diagnosticImaging.PIQIScore, 0);
        Assert.Equal(diagnosticImaging.Numerator, 0);
        Assert.Equal(diagnosticImaging.Denominator, 0);
        Assert.Equal(diagnosticImaging.CriticalFailureCount, 0);
        Assert.Equal(diagnosticImaging.WeightedPIQIScore, 0);
        Assert.Equal(diagnosticImaging.WeightedNumerator, 0);
        Assert.Equal(diagnosticImaging.WeightedDenominator, 0);
        #endregion

        #region Encounters
        var encounters = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Encounters");
        Assert.Equal(encounters.InstanceCount, 1);
        Assert.Equal(encounters.PIQIScore, 40);
        Assert.Equal(encounters.Numerator, 2);
        Assert.Equal(encounters.Denominator, 5);
        Assert.Equal(encounters.CriticalFailureCount, 0);
        Assert.Equal(encounters.WeightedPIQIScore, 40);
        Assert.Equal(encounters.WeightedNumerator, 2);
        Assert.Equal(encounters.WeightedDenominator, 5);
        #endregion

        #region Goals
        var goals = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Goals");
        Assert.Equal(goals.InstanceCount, 0);
        Assert.Equal(goals.PIQIScore, 0);
        Assert.Equal(goals.Numerator, 0);
        Assert.Equal(goals.Denominator, 0);
        Assert.Equal(goals.CriticalFailureCount, 0);
        Assert.Equal(goals.WeightedPIQIScore, 0);
        Assert.Equal(goals.WeightedNumerator, 0);
        Assert.Equal(goals.WeightedDenominator, 0);
        #endregion

        #region Health Assessments
        var healthAssessments = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Health Assessments");
        Assert.Equal(healthAssessments.InstanceCount, 1);
        Assert.Equal(healthAssessments.PIQIScore, 50);
        Assert.Equal(healthAssessments.Numerator, 1);
        Assert.Equal(healthAssessments.Denominator, 2);
        Assert.Equal(healthAssessments.CriticalFailureCount, 0);
        Assert.Equal(healthAssessments.WeightedPIQIScore, 50);
        Assert.Equal(healthAssessments.WeightedNumerator, 1);
        Assert.Equal(healthAssessments.WeightedDenominator, 2);
        #endregion

        #region Immunizations
        var immunizations = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Immunizations");
        Assert.Equal(immunizations.InstanceCount, 0);
        Assert.Equal(immunizations.PIQIScore, 0);
        Assert.Equal(immunizations.Numerator, 0);
        Assert.Equal(immunizations.Denominator, 0);
        Assert.Equal(immunizations.CriticalFailureCount, 0);
        Assert.Equal(immunizations.WeightedPIQIScore, 0);
        Assert.Equal(immunizations.WeightedNumerator, 0);
        Assert.Equal(immunizations.WeightedDenominator, 0);
        #endregion

        #region Lab Results
        var labResults = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Lab Results");
        Assert.Equal(labResults.InstanceCount, 0);
        Assert.Equal(labResults.PIQIScore, 0);
        Assert.Equal(labResults.Numerator, 0);
        Assert.Equal(labResults.Denominator, 0);
        Assert.Equal(labResults.CriticalFailureCount, 0);
        Assert.Equal(labResults.WeightedPIQIScore, 0);
        Assert.Equal(labResults.WeightedNumerator, 0);
        Assert.Equal(labResults.WeightedDenominator, 0);
        #endregion

        #region Medical Devices
        var medicalDevices = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Medical Devices");
        Assert.Equal(medicalDevices.InstanceCount, 0);
        Assert.Equal(medicalDevices.PIQIScore, 0);
        Assert.Equal(medicalDevices.Numerator, 0);
        Assert.Equal(medicalDevices.Denominator, 0);
        Assert.Equal(medicalDevices.CriticalFailureCount, 0);
        Assert.Equal(medicalDevices.WeightedPIQIScore, 0);
        Assert.Equal(medicalDevices.WeightedNumerator, 0);
        Assert.Equal(medicalDevices.WeightedDenominator, 0);
        #endregion

        #region Medications
        var medications = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Medications");
        Assert.Equal(medications.InstanceCount, 0);
        Assert.Equal(medications.PIQIScore, 0);
        Assert.Equal(medications.Numerator, 0);
        Assert.Equal(medications.Denominator, 0);
        Assert.Equal(medications.CriticalFailureCount, 0);
        Assert.Equal(medications.WeightedPIQIScore, 0);
        Assert.Equal(medications.WeightedNumerator, 0);
        Assert.Equal(medications.WeightedDenominator, 0);
        #endregion

        #region Procedures
        var procedures = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Procedures");
        Assert.Equal(procedures.InstanceCount, 0);
        Assert.Equal(procedures.PIQIScore, 0);
        Assert.Equal(procedures.Numerator, 0);
        Assert.Equal(procedures.Denominator, 0);
        Assert.Equal(procedures.CriticalFailureCount, 0);
        Assert.Equal(procedures.WeightedPIQIScore, 0);
        Assert.Equal(procedures.WeightedNumerator, 0);
        Assert.Equal(procedures.WeightedDenominator, 0);
        #endregion

        #region Vital Signs
        var vitalSigns = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Vital Signs");
        Assert.Equal(vitalSigns.InstanceCount, 0);
        Assert.Equal(vitalSigns.PIQIScore, 0);
        Assert.Equal(vitalSigns.Numerator, 0);
        Assert.Equal(vitalSigns.Denominator, 0);
        Assert.Equal(vitalSigns.CriticalFailureCount, 0);
        Assert.Equal(vitalSigns.WeightedPIQIScore, 0);
        Assert.Equal(vitalSigns.WeightedNumerator, 0);
        Assert.Equal(vitalSigns.WeightedDenominator, 0);
        #endregion

        #endregion

        #region Informational Results
        Assert.Equal(result.ScoringData.InformationalResults.Count, 15);

        #region Allergies
        var allergiesInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Allergies");
        Assert.Equal(allergiesInfo.EvaluationList.Count, 0);
        #endregion

        #region Clinical Documents
        var clinicalDocumentsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Clinical Documents");
        Assert.Equal(clinicalDocumentsInfo.EvaluationList.Count, 0);
        #endregion

        #region Conditions
        var conditionsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Conditions");
        Assert.Equal(conditionsInfo.EvaluationList.Count, 0);
        #endregion

        #region Demographics
        var demographicsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Demographics");
        Assert.Equal(demographicsInfo.EvaluationList.Count, 0);
        #endregion

        #region Diagnostic Imaging
        var diagnosticImagingInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Diagnostic Imaging");
        Assert.Equal(diagnosticImagingInfo.EvaluationList.Count, 0);
        #endregion

        #region Encounters
        var encountersInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Encounters");
        Assert.Equal(encountersInfo.EvaluationList.Count, 0);
        #endregion

        #region Goals
        var goalsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Goals");
        Assert.Equal(goalsInfo.EvaluationList.Count, 0);
        #endregion

        #region Health Assessments
        var healthAssessmentsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Health Assessments");
        Assert.Equal(healthAssessmentsInfo.EvaluationList.Count, 0);
        #endregion

        #region Immunizations
        var immunizationsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Immunizations");
        Assert.Equal(immunizationsInfo.EvaluationList.Count, 0);
        #endregion

        #region Lab Results
        var labResultsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Lab Results");
        Assert.Equal(labResultsInfo.EvaluationList.Count, 0);
        #endregion

        #region Medical Devices
        var medicalDevicesInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Medical Devices");
        Assert.Equal(medicalDevicesInfo.EvaluationList.Count, 0);
        #endregion

        #region Medications
        var medicationsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Medications");
        Assert.Equal(medicationsInfo.EvaluationList.Count, 0);
        #endregion

        #region Procedures
        var proceduresInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Procedures");
        Assert.Equal(proceduresInfo.EvaluationList.Count, 0);
        #endregion

        #region Vital Signs
        var vitalSignsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Vital Signs");
        Assert.Equal(vitalSignsInfo.EvaluationList.Count, 0);
        #endregion

        #endregion

        #endregion
    }

    [Theory]
    [InlineData("/PIQI/ScoreAuditMessage")]
    public async Task ScoreAuditMessage1_ReturnsExpectedResponse(string endpoint)
    {
        // Arrange
        var piqiRequest = new PIQIRequest
        {
            DataProviderID = "TestProvider",
            DataSourceID = "TestSource",
            PIQIModelMnemonic = "PAT_CLINICAL_V1",
            EvaluationRubricMnemonic = "USCDI_V3",
            MessageID = "Msg001",
            MessageData = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData/Test1_PIQI.json"))
        };
        var result = new PIQIResponse();
        var requestContent = new StringContent(JsonConvert.SerializeObject(piqiRequest), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(endpoint, requestContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var contentType = response.Content.Headers.ContentType.MediaType;
        if (contentType == "text/plain" || contentType == "application/json")
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            result = JsonConvert.DeserializeObject<PIQIResponse>(responseBody);

            Assert.NotNull(result);
        }

        #region Check Results

        #region Overall Message Results
        Assert.Equal(result.Succeeded, true);
        Assert.Equal(result.ScoringData.MessageResults.PIQIScore, 54);
        Assert.Equal(result.ScoringData.MessageResults.Numerator, 6);
        Assert.Equal(result.ScoringData.MessageResults.Denominator, 11);
        Assert.Equal(result.ScoringData.MessageResults.CriticalFailureCount, 0);
        Assert.Equal(result.ScoringData.MessageResults.WeightedPIQIScore, 54);
        Assert.Equal(result.ScoringData.MessageResults.WeightedNumerator, 6);
        Assert.Equal(result.ScoringData.MessageResults.WeightedDenominator, 11);
        #endregion

        #region Data Class Results
        Assert.Equal(result.ScoringData.DataClassResults.Count, 15);

        #region Allergies
        var allergies = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Allergies");
        Assert.Equal(allergies.InstanceCount, 0);
        Assert.Equal(allergies.PIQIScore, 0);
        Assert.Equal(allergies.Numerator, 0);
        Assert.Equal(allergies.Denominator, 0);
        Assert.Equal(allergies.CriticalFailureCount, 0);
        Assert.Equal(allergies.WeightedPIQIScore, 0);
        Assert.Equal(allergies.WeightedNumerator, 0);
        Assert.Equal(allergies.WeightedDenominator, 0);
        #endregion

        #region Clinical Documents
        var clinicalDocuments = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Clinical Documents");
        Assert.Equal(clinicalDocuments.InstanceCount, 0);
        Assert.Equal(clinicalDocuments.PIQIScore, 0);
        Assert.Equal(clinicalDocuments.Numerator, 0);
        Assert.Equal(clinicalDocuments.Denominator, 0);
        Assert.Equal(clinicalDocuments.CriticalFailureCount, 0);
        Assert.Equal(clinicalDocuments.WeightedPIQIScore, 0);
        Assert.Equal(clinicalDocuments.WeightedNumerator, 0);
        Assert.Equal(clinicalDocuments.WeightedDenominator, 0);
        #endregion

        #region Conditions
        var conditions = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Conditions");
        Assert.Equal(conditions.InstanceCount, 0);
        Assert.Equal(conditions.PIQIScore, 0);
        Assert.Equal(conditions.Numerator, 0);
        Assert.Equal(conditions.Denominator, 0);
        Assert.Equal(conditions.CriticalFailureCount, 0);
        Assert.Equal(conditions.WeightedPIQIScore, 0);
        Assert.Equal(conditions.WeightedNumerator, 0);
        Assert.Equal(conditions.WeightedDenominator, 0);
        #endregion

        #region Demographics
        var demographics = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Demographics");
        Assert.Equal(demographics.InstanceCount, 1);
        Assert.Equal(demographics.PIQIScore, 75);
        Assert.Equal(demographics.Numerator, 3);
        Assert.Equal(demographics.Denominator, 4);
        Assert.Equal(demographics.CriticalFailureCount, 0);
        Assert.Equal(demographics.WeightedPIQIScore, 75);
        Assert.Equal(demographics.WeightedNumerator, 3);
        Assert.Equal(demographics.WeightedDenominator, 4);
        #endregion

        #region Diagnostic Imaging
        var diagnosticImaging = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Diagnostic Imaging");
        Assert.Equal(diagnosticImaging.InstanceCount, 0);
        Assert.Equal(diagnosticImaging.PIQIScore, 0);
        Assert.Equal(diagnosticImaging.Numerator, 0);
        Assert.Equal(diagnosticImaging.Denominator, 0);
        Assert.Equal(diagnosticImaging.CriticalFailureCount, 0);
        Assert.Equal(diagnosticImaging.WeightedPIQIScore, 0);
        Assert.Equal(diagnosticImaging.WeightedNumerator, 0);
        Assert.Equal(diagnosticImaging.WeightedDenominator, 0);
        #endregion

        #region Encounters
        var encounters = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Encounters");
        Assert.Equal(encounters.InstanceCount, 1);
        Assert.Equal(encounters.PIQIScore, 40);
        Assert.Equal(encounters.Numerator, 2);
        Assert.Equal(encounters.Denominator, 5);
        Assert.Equal(encounters.CriticalFailureCount, 0);
        Assert.Equal(encounters.WeightedPIQIScore, 40);
        Assert.Equal(encounters.WeightedNumerator, 2);
        Assert.Equal(encounters.WeightedDenominator, 5);
        #endregion

        #region Goals
        var goals = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Goals");
        Assert.Equal(goals.InstanceCount, 0);
        Assert.Equal(goals.PIQIScore, 0);
        Assert.Equal(goals.Numerator, 0);
        Assert.Equal(goals.Denominator, 0);
        Assert.Equal(goals.CriticalFailureCount, 0);
        Assert.Equal(goals.WeightedPIQIScore, 0);
        Assert.Equal(goals.WeightedNumerator, 0);
        Assert.Equal(goals.WeightedDenominator, 0);
        #endregion

        #region Health Assessments
        var healthAssessments = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Health Assessments");
        Assert.Equal(healthAssessments.InstanceCount, 1);
        Assert.Equal(healthAssessments.PIQIScore, 50);
        Assert.Equal(healthAssessments.Numerator, 1);
        Assert.Equal(healthAssessments.Denominator, 2);
        Assert.Equal(healthAssessments.CriticalFailureCount, 0);
        Assert.Equal(healthAssessments.WeightedPIQIScore, 50);
        Assert.Equal(healthAssessments.WeightedNumerator, 1);
        Assert.Equal(healthAssessments.WeightedDenominator, 2);
        #endregion

        #region Immunizations
        var immunizations = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Immunizations");
        Assert.Equal(immunizations.InstanceCount, 0);
        Assert.Equal(immunizations.PIQIScore, 0);
        Assert.Equal(immunizations.Numerator, 0);
        Assert.Equal(immunizations.Denominator, 0);
        Assert.Equal(immunizations.CriticalFailureCount, 0);
        Assert.Equal(immunizations.WeightedPIQIScore, 0);
        Assert.Equal(immunizations.WeightedNumerator, 0);
        Assert.Equal(immunizations.WeightedDenominator, 0);
        #endregion

        #region Lab Results
        var labResults = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Lab Results");
        Assert.Equal(labResults.InstanceCount, 0);
        Assert.Equal(labResults.PIQIScore, 0);
        Assert.Equal(labResults.Numerator, 0);
        Assert.Equal(labResults.Denominator, 0);
        Assert.Equal(labResults.CriticalFailureCount, 0);
        Assert.Equal(labResults.WeightedPIQIScore, 0);
        Assert.Equal(labResults.WeightedNumerator, 0);
        Assert.Equal(labResults.WeightedDenominator, 0);
        #endregion

        #region Medical Devices
        var medicalDevices = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Medical Devices");
        Assert.Equal(medicalDevices.InstanceCount, 0);
        Assert.Equal(medicalDevices.PIQIScore, 0);
        Assert.Equal(medicalDevices.Numerator, 0);
        Assert.Equal(medicalDevices.Denominator, 0);
        Assert.Equal(medicalDevices.CriticalFailureCount, 0);
        Assert.Equal(medicalDevices.WeightedPIQIScore, 0);
        Assert.Equal(medicalDevices.WeightedNumerator, 0);
        Assert.Equal(medicalDevices.WeightedDenominator, 0);
        #endregion

        #region Medications
        var medications = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Medications");
        Assert.Equal(medications.InstanceCount, 0);
        Assert.Equal(medications.PIQIScore, 0);
        Assert.Equal(medications.Numerator, 0);
        Assert.Equal(medications.Denominator, 0);
        Assert.Equal(medications.CriticalFailureCount, 0);
        Assert.Equal(medications.WeightedPIQIScore, 0);
        Assert.Equal(medications.WeightedNumerator, 0);
        Assert.Equal(medications.WeightedDenominator, 0);
        #endregion

        #region Procedures
        var procedures = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Procedures");
        Assert.Equal(procedures.InstanceCount, 0);
        Assert.Equal(procedures.PIQIScore, 0);
        Assert.Equal(procedures.Numerator, 0);
        Assert.Equal(procedures.Denominator, 0);
        Assert.Equal(procedures.CriticalFailureCount, 0);
        Assert.Equal(procedures.WeightedPIQIScore, 0);
        Assert.Equal(procedures.WeightedNumerator, 0);
        Assert.Equal(procedures.WeightedDenominator, 0);
        #endregion

        #region Vital Signs
        var vitalSigns = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Vital Signs");
        Assert.Equal(vitalSigns.InstanceCount, 0);
        Assert.Equal(vitalSigns.PIQIScore, 0);
        Assert.Equal(vitalSigns.Numerator, 0);
        Assert.Equal(vitalSigns.Denominator, 0);
        Assert.Equal(vitalSigns.CriticalFailureCount, 0);
        Assert.Equal(vitalSigns.WeightedPIQIScore, 0);
        Assert.Equal(vitalSigns.WeightedNumerator, 0);
        Assert.Equal(vitalSigns.WeightedDenominator, 0);
        #endregion

        #endregion

        #region Informational Results
        Assert.Equal(result.ScoringData.InformationalResults.Count, 15);

        #region Allergies
        var allergiesInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Allergies");
        Assert.Equal(allergiesInfo.EvaluationList.Count, 0);
        #endregion

        #region Clinical Documents
        var clinicalDocumentsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Clinical Documents");
        Assert.Equal(clinicalDocumentsInfo.EvaluationList.Count, 0);
        #endregion

        #region Conditions
        var conditionsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Conditions");
        Assert.Equal(conditionsInfo.EvaluationList.Count, 0);
        #endregion

        #region Demographics
        var demographicsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Demographics");
        Assert.Equal(demographicsInfo.EvaluationList.Count, 0);
        #endregion

        #region Diagnostic Imaging
        var diagnosticImagingInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Diagnostic Imaging");
        Assert.Equal(diagnosticImagingInfo.EvaluationList.Count, 0);
        #endregion

        #region Encounters
        var encountersInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Encounters");
        Assert.Equal(encountersInfo.EvaluationList.Count, 0);
        #endregion

        #region Goals
        var goalsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Goals");
        Assert.Equal(goalsInfo.EvaluationList.Count, 0);
        #endregion

        #region Health Assessments
        var healthAssessmentsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Health Assessments");
        Assert.Equal(healthAssessmentsInfo.EvaluationList.Count, 0);
        #endregion

        #region Immunizations
        var immunizationsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Immunizations");
        Assert.Equal(immunizationsInfo.EvaluationList.Count, 0);
        #endregion

        #region Lab Results
        var labResultsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Lab Results");
        Assert.Equal(labResultsInfo.EvaluationList.Count, 0);
        #endregion

        #region Medical Devices
        var medicalDevicesInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Medical Devices");
        Assert.Equal(medicalDevicesInfo.EvaluationList.Count, 0);
        #endregion

        #region Medications
        var medicationsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Medications");
        Assert.Equal(medicationsInfo.EvaluationList.Count, 0);
        #endregion

        #region Procedures
        var proceduresInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Procedures");
        Assert.Equal(proceduresInfo.EvaluationList.Count, 0);
        #endregion

        #region Vital Signs
        var vitalSignsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Vital Signs");
        Assert.Equal(vitalSignsInfo.EvaluationList.Count, 0);
        #endregion

        #endregion

        #region Audit Results
        Assert.Equal(Regex.Replace(result.AuditedMessage, @"\s+", ""), Regex.Replace("{\r\n  \"EntityModelMnemonic\": \"PAT_CLINICAL_V1\",\r\n  \"DataProviderID\": \"TestProvider\",\r\n  \"DataSourceID\": \"TestSource\",\r\n  \"MessageID\": \"Msg001\",\r\n  \"Audit\": {\r\n    \"messageNumerator\": \"6\",\r\n    \"messageDenominator\": \"11\",\r\n    \"messageScore\": \"54\",\r\n    \"messageNumeratorWeighted\": \"6\",\r\n    \"messageDenominatorWeighted\": \"11\",\r\n    \"messageScoreWeighted\": \"54\",\r\n    \"messageCriticalFailureCount\": \"0\"\r\n  },\r\n  \"patient\": {\r\n    \"allergies\": [],\r\n    \"clinicalDocuments\": [],\r\n    \"conditions\": [],\r\n    \"demographics\": [\r\n      {\r\n        \"birthDate\": {\r\n          \"data\": \"2009-01-01\",\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"100\",\r\n              \"attributeScoreWeighted\": \"100\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"1\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"DEM_DOB\",\r\n                \"attributeName\": \"birthDate\",\r\n                \"assessment\": \"Date of birth is valid past date\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Passed\",\r\n                \"reason\": \"\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"birthSex\": {\r\n          \"data\": {\r\n            \"codings\": [\r\n              {\r\n                \"system\": \"http://hl7.org/fhir/administrative-gender\",\r\n                \"code\": \"male\",\r\n                \"display\": \"male\"\r\n              }\r\n            ],\r\n            \"text\": \"male\"\r\n          },\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"DEM_SEX\",\r\n                \"attributeName\": \"birthSex\",\r\n                \"assessment\": \"Birth sex is SNOMED-CT\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"invalid concept\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"deathDate\": {},\r\n        \"deceased\": {},\r\n        \"ethnicity\": {\r\n          \"data\": {\r\n            \"codings\": [\r\n              {\r\n                \"system\": \"urn:oid:2.16.840.1.113883.6.238\",\r\n                \"code\": \"2135-2\",\r\n                \"display\": \"Hispanic or Latino\"\r\n              }\r\n            ],\r\n            \"text\": \"Hispanic or Latino\"\r\n          },\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"100\",\r\n              \"attributeScoreWeighted\": \"100\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"1\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"DEM_ETHN\",\r\n                \"attributeName\": \"ethnicity\",\r\n                \"assessment\": \"Ethnicity is valid code\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Passed\",\r\n                \"reason\": \"\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"genderIdentity\": {},\r\n        \"maritalStatus\": {},\r\n        \"patientIdentifier\": {},\r\n        \"primaryLanguage\": {},\r\n        \"race\": {\r\n          \"data\": {\r\n            \"codings\": [\r\n              {\r\n                \"system\": \"urn:oid:2.16.840.1.113883.6.238\",\r\n                \"code\": \"1002-5\",\r\n                \"display\": \"American Indian or Alaska Native\"\r\n              }\r\n            ],\r\n            \"text\": \"American Indian or Alaska Native\"\r\n          },\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"100\",\r\n              \"attributeScoreWeighted\": \"100\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"1\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"DEM_RACE\",\r\n                \"attributeName\": \"race\",\r\n                \"assessment\": \"Race is valid concept\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Passed\",\r\n                \"reason\": \"\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"elementAudit\": {\r\n          \"elementScore\": \"75\",\r\n          \"elementScoreWeighted\": \"75\",\r\n          \"elementCriticalFailureCount\": \"0\",\r\n          \"elementNumerator\": \"3\",\r\n          \"elementDenominator\": \"4\"\r\n        }\r\n      }\r\n    ],\r\n    \"diagnosticImaging\": [],\r\n    \"encounters\": [\r\n      {\r\n        \"encounterDateTime\": {\r\n          \"data\": \"1/2/2026 3:00:00 AM\",\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"100\",\r\n              \"attributeScoreWeighted\": \"100\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"1\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"ENC_DATETIME\",\r\n                \"attributeName\": \"encounter date/time\",\r\n                \"assessment\": \"Encounter date is in the past\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Passed\",\r\n                \"reason\": \"\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"encounterDiagnosis\": {\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"ENC_DIAGNOSIS\",\r\n                \"attributeName\": \"encounter diagnosis\",\r\n                \"assessment\": \"Diagnosis is SNOMED-CT or ICD-10-CM\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"unpopulated\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"encounterDisposition\": {\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"ENC_DISPOSITION\",\r\n                \"attributeName\": \"encounter disposition\",\r\n                \"assessment\": \"Encounter disposition is populated\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"Encounter disposition is not populated\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"encounterEndDateTime\": {},\r\n        \"encounterIdentifier\": {},\r\n        \"encounterLocation\": {\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"ENC_LOCATION\",\r\n                \"attributeName\": \"encounter location\",\r\n                \"assessment\": \"Encounter location is populated\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"Encounter location is not populated\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"encounterReason\": {},\r\n        \"encounterStatus\": {},\r\n        \"encounterType\": {\r\n          \"data\": {\r\n            \"text\": \"Psychiatric interview and evaluation (procedure)\"\r\n          },\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"100\",\r\n              \"attributeScoreWeighted\": \"100\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"1\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"ENC_TYPE\",\r\n                \"attributeName\": \"encounter type\",\r\n                \"assessment\": \"Encounter type is populated\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Passed\",\r\n                \"reason\": \"\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"elementAudit\": {\r\n          \"elementScore\": \"40\",\r\n          \"elementScoreWeighted\": \"40\",\r\n          \"elementCriticalFailureCount\": \"0\",\r\n          \"elementNumerator\": \"2\",\r\n          \"elementDenominator\": \"5\"\r\n        }\r\n      }\r\n    ],\r\n    \"goals\": [],\r\n    \"healthAssessments\": [\r\n      {\r\n        \"assessment\": {\r\n          \"data\": {\r\n            \"codings\": [\r\n              {\r\n                \"system\": \"http://loinc.org\",\r\n                \"code\": \"73831-0\",\r\n                \"display\": \"Adolescent depression screening assessment\"\r\n              }\r\n            ],\r\n            \"text\": \"Adolescent depression screening assessment\"\r\n          },\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"100\",\r\n              \"attributeScoreWeighted\": \"100\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"1\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"HA_ITEM\",\r\n                \"attributeName\": \"assessment\",\r\n                \"assessment\": \"Health status assessment is  LOINC or SNOMED-CT\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Passed\",\r\n                \"reason\": \"\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"effectiveDate\": {\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"HA_EFFDT\",\r\n                \"attributeName\": \"effectiveDate\",\r\n                \"assessment\": \"Health Status assessment date is date in past\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"unpopulated\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"issueDateTime\": {},\r\n        \"resultUnit\": {},\r\n        \"resultValue\": {\r\n          \"data\": {\r\n            \"text\": \"true\",\r\n            \"type\": {\r\n              \"text\": \"ST\"\r\n            }\r\n          }\r\n        },\r\n        \"elementAudit\": {\r\n          \"elementScore\": \"50\",\r\n          \"elementScoreWeighted\": \"50\",\r\n          \"elementCriticalFailureCount\": \"0\",\r\n          \"elementNumerator\": \"1\",\r\n          \"elementDenominator\": \"2\"\r\n        }\r\n      }\r\n    ],\r\n    \"immunizations\": [],\r\n    \"labResults\": [],\r\n    \"medicalDevices\": [],\r\n    \"medications\": [],\r\n    \"procedures\": [],\r\n    \"providers\": [],\r\n    \"vitalSigns\": []\r\n  }\r\n}", @"\s+", ""));
        #endregion

        #endregion
    }

    [Theory]
    [InlineData("/PIQI/ScoreMessage")]
    public async Task ScoresMessage2_ReturnsExpectedResponse(string endpoint)
    {
        // Arrange
        var piqiRequest = new PIQIRequest
        {
            DataProviderID = "TestProvider",
            DataSourceID = "TestSource",
            PIQIModelMnemonic = "PAT_CLINICAL_V1",
            EvaluationRubricMnemonic = "USCDI_V3",
            MessageID = "Msg002",
            MessageData = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData/Test2_PIQI.json"))
        };
        var result = new PIQIResponse();
        var requestContent = new StringContent(JsonConvert.SerializeObject(piqiRequest), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(endpoint, requestContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var contentType = response.Content.Headers.ContentType.MediaType;
        if (contentType == "text/plain" || contentType == "application/json")
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            result = JsonConvert.DeserializeObject<PIQIResponse>(responseBody);

            Assert.NotNull(result);
        }

        #region Check Results

        #region Overall Message Results
        Assert.Equal(result.Succeeded, true);
        Assert.Equal(result.ScoringData.MessageResults.PIQIScore, 44);
        Assert.Equal(result.ScoringData.MessageResults.Numerator, 4);
        Assert.Equal(result.ScoringData.MessageResults.Denominator, 9);
        Assert.Equal(result.ScoringData.MessageResults.CriticalFailureCount, 0);
        Assert.Equal(result.ScoringData.MessageResults.WeightedPIQIScore, 44);
        Assert.Equal(result.ScoringData.MessageResults.WeightedNumerator, 4);
        Assert.Equal(result.ScoringData.MessageResults.WeightedDenominator, 9);
        #endregion

        #region Data Class Results
        Assert.Equal(result.ScoringData.DataClassResults.Count, 15);

        #region Allergies
        var allergies = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Allergies");
        Assert.Equal(allergies.InstanceCount, 0);
        Assert.Equal(allergies.PIQIScore, 0);
        Assert.Equal(allergies.Numerator, 0);
        Assert.Equal(allergies.Denominator, 0);
        Assert.Equal(allergies.CriticalFailureCount, 0);
        Assert.Equal(allergies.WeightedPIQIScore, 0);
        Assert.Equal(allergies.WeightedNumerator, 0);
        Assert.Equal(allergies.WeightedDenominator, 0);
        #endregion

        #region Clinical Documents
        var clinicalDocuments = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Clinical Documents");
        Assert.Equal(clinicalDocuments.InstanceCount, 0);
        Assert.Equal(clinicalDocuments.PIQIScore, 0);
        Assert.Equal(clinicalDocuments.Numerator, 0);
        Assert.Equal(clinicalDocuments.Denominator, 0);
        Assert.Equal(clinicalDocuments.CriticalFailureCount, 0);
        Assert.Equal(clinicalDocuments.WeightedPIQIScore, 0);
        Assert.Equal(clinicalDocuments.WeightedNumerator, 0);
        Assert.Equal(clinicalDocuments.WeightedDenominator, 0);
        #endregion

        #region Conditions
        var conditions = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Conditions");
        Assert.Equal(conditions.InstanceCount, 0);
        Assert.Equal(conditions.PIQIScore, 0);
        Assert.Equal(conditions.Numerator, 0);
        Assert.Equal(conditions.Denominator, 0);
        Assert.Equal(conditions.CriticalFailureCount, 0);
        Assert.Equal(conditions.WeightedPIQIScore, 0);
        Assert.Equal(conditions.WeightedNumerator, 0);
        Assert.Equal(conditions.WeightedDenominator, 0);
        #endregion

        #region Demographics
        var demographics = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Demographics");
        Assert.Equal(demographics.InstanceCount, 1);
        Assert.Equal(demographics.PIQIScore, 75);
        Assert.Equal(demographics.Numerator, 3);
        Assert.Equal(demographics.Denominator, 4);
        Assert.Equal(demographics.CriticalFailureCount, 0);
        Assert.Equal(demographics.WeightedPIQIScore, 75);
        Assert.Equal(demographics.WeightedNumerator, 3);
        Assert.Equal(demographics.WeightedDenominator, 4);
        #endregion

        #region Diagnostic Imaging
        var diagnosticImaging = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Diagnostic Imaging");
        Assert.Equal(diagnosticImaging.InstanceCount, 0);
        Assert.Equal(diagnosticImaging.PIQIScore, 0);
        Assert.Equal(diagnosticImaging.Numerator, 0);
        Assert.Equal(diagnosticImaging.Denominator, 0);
        Assert.Equal(diagnosticImaging.CriticalFailureCount, 0);
        Assert.Equal(diagnosticImaging.WeightedPIQIScore, 0);
        Assert.Equal(diagnosticImaging.WeightedNumerator, 0);
        Assert.Equal(diagnosticImaging.WeightedDenominator, 0);
        #endregion

        #region Encounters
        var encounters = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Encounters");
        Assert.Equal(encounters.InstanceCount, 1);
        Assert.Equal(encounters.PIQIScore, 20);
        Assert.Equal(encounters.Numerator, 1);
        Assert.Equal(encounters.Denominator, 5);
        Assert.Equal(encounters.CriticalFailureCount, 0);
        Assert.Equal(encounters.WeightedPIQIScore, 20);
        Assert.Equal(encounters.WeightedNumerator, 1);
        Assert.Equal(encounters.WeightedDenominator, 5);
        #endregion

        #region Goals
        var goals = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Goals");
        Assert.Equal(goals.InstanceCount, 0);
        Assert.Equal(goals.PIQIScore, 0);
        Assert.Equal(goals.Numerator, 0);
        Assert.Equal(goals.Denominator, 0);
        Assert.Equal(goals.CriticalFailureCount, 0);
        Assert.Equal(goals.WeightedPIQIScore, 0);
        Assert.Equal(goals.WeightedNumerator, 0);
        Assert.Equal(goals.WeightedDenominator, 0);
        #endregion

        #region Health Assessments
        var healthAssessments = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Health Assessments");
        Assert.Equal(healthAssessments.InstanceCount, 0);
        Assert.Equal(healthAssessments.PIQIScore, 0);
        Assert.Equal(healthAssessments.Numerator, 0);
        Assert.Equal(healthAssessments.Denominator, 0);
        Assert.Equal(healthAssessments.CriticalFailureCount, 0);
        Assert.Equal(healthAssessments.WeightedPIQIScore, 0);
        Assert.Equal(healthAssessments.WeightedNumerator, 0);
        Assert.Equal(healthAssessments.WeightedDenominator, 0);
        #endregion

        #region Immunizations
        var immunizations = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Immunizations");
        Assert.Equal(immunizations.InstanceCount, 0);
        Assert.Equal(immunizations.PIQIScore, 0);
        Assert.Equal(immunizations.Numerator, 0);
        Assert.Equal(immunizations.Denominator, 0);
        Assert.Equal(immunizations.CriticalFailureCount, 0);
        Assert.Equal(immunizations.WeightedPIQIScore, 0);
        Assert.Equal(immunizations.WeightedNumerator, 0);
        Assert.Equal(immunizations.WeightedDenominator, 0);
        #endregion

        #region Lab Results
        var labResults = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Lab Results");
        Assert.Equal(labResults.InstanceCount, 0);
        Assert.Equal(labResults.PIQIScore, 0);
        Assert.Equal(labResults.Numerator, 0);
        Assert.Equal(labResults.Denominator, 0);
        Assert.Equal(labResults.CriticalFailureCount, 0);
        Assert.Equal(labResults.WeightedPIQIScore, 0);
        Assert.Equal(labResults.WeightedNumerator, 0);
        Assert.Equal(labResults.WeightedDenominator, 0);
        #endregion

        #region Medical Devices
        var medicalDevices = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Medical Devices");
        Assert.Equal(medicalDevices.InstanceCount, 0);
        Assert.Equal(medicalDevices.PIQIScore, 0);
        Assert.Equal(medicalDevices.Numerator, 0);
        Assert.Equal(medicalDevices.Denominator, 0);
        Assert.Equal(medicalDevices.CriticalFailureCount, 0);
        Assert.Equal(medicalDevices.WeightedPIQIScore, 0);
        Assert.Equal(medicalDevices.WeightedNumerator, 0);
        Assert.Equal(medicalDevices.WeightedDenominator, 0);
        #endregion

        #region Medications
        var medications = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Medications");
        Assert.Equal(medications.InstanceCount, 0);
        Assert.Equal(medications.PIQIScore, 0);
        Assert.Equal(medications.Numerator, 0);
        Assert.Equal(medications.Denominator, 0);
        Assert.Equal(medications.CriticalFailureCount, 0);
        Assert.Equal(medications.WeightedPIQIScore, 0);
        Assert.Equal(medications.WeightedNumerator, 0);
        Assert.Equal(medications.WeightedDenominator, 0);
        #endregion

        #region Procedures
        var procedures = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Procedures");
        Assert.Equal(procedures.InstanceCount, 0);
        Assert.Equal(procedures.PIQIScore, 0);
        Assert.Equal(procedures.Numerator, 0);
        Assert.Equal(procedures.Denominator, 0);
        Assert.Equal(procedures.CriticalFailureCount, 0);
        Assert.Equal(procedures.WeightedPIQIScore, 0);
        Assert.Equal(procedures.WeightedNumerator, 0);
        Assert.Equal(procedures.WeightedDenominator, 0);
        #endregion

        #region Vital Signs
        var vitalSigns = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Vital Signs");
        Assert.Equal(vitalSigns.InstanceCount, 0);
        Assert.Equal(vitalSigns.PIQIScore, 0);
        Assert.Equal(vitalSigns.Numerator, 0);
        Assert.Equal(vitalSigns.Denominator, 0);
        Assert.Equal(vitalSigns.CriticalFailureCount, 0);
        Assert.Equal(vitalSigns.WeightedPIQIScore, 0);
        Assert.Equal(vitalSigns.WeightedNumerator, 0);
        Assert.Equal(vitalSigns.WeightedDenominator, 0);
        #endregion

        #endregion

        #region Informational Results
        Assert.Equal(result.ScoringData.InformationalResults.Count, 15);

        #region Allergies
        var allergiesInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Allergies");
        Assert.Equal(allergiesInfo.EvaluationList.Count, 0);
        #endregion

        #region Clinical Documents
        var clinicalDocumentsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Clinical Documents");
        Assert.Equal(clinicalDocumentsInfo.EvaluationList.Count, 0);
        #endregion

        #region Conditions
        var conditionsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Conditions");
        Assert.Equal(conditionsInfo.EvaluationList.Count, 0);
        #endregion

        #region Demographics
        var demographicsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Demographics");
        Assert.Equal(demographicsInfo.EvaluationList.Count, 0);
        #endregion

        #region Diagnostic Imaging
        var diagnosticImagingInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Diagnostic Imaging");
        Assert.Equal(diagnosticImagingInfo.EvaluationList.Count, 0);
        #endregion

        #region Encounters
        var encountersInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Encounters");
        Assert.Equal(encountersInfo.EvaluationList.Count, 0);
        #endregion

        #region Goals
        var goalsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Goals");
        Assert.Equal(goalsInfo.EvaluationList.Count, 0);
        #endregion

        #region Health Assessments
        var healthAssessmentsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Health Assessments");
        Assert.Equal(healthAssessmentsInfo.EvaluationList.Count, 0);
        #endregion

        #region Immunizations
        var immunizationsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Immunizations");
        Assert.Equal(immunizationsInfo.EvaluationList.Count, 0);
        #endregion

        #region Lab Results
        var labResultsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Lab Results");
        Assert.Equal(labResultsInfo.EvaluationList.Count, 0);
        #endregion

        #region Medical Devices
        var medicalDevicesInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Medical Devices");
        Assert.Equal(medicalDevicesInfo.EvaluationList.Count, 0);
        #endregion

        #region Medications
        var medicationsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Medications");
        Assert.Equal(medicationsInfo.EvaluationList.Count, 0);
        #endregion

        #region Procedures
        var proceduresInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Procedures");
        Assert.Equal(proceduresInfo.EvaluationList.Count, 0);
        #endregion

        #region Vital Signs
        var vitalSignsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Vital Signs");
        Assert.Equal(vitalSignsInfo.EvaluationList.Count, 0);
        #endregion

        #endregion

        #endregion
    }

    [Theory]
    [InlineData("/PIQI/ScoreAuditMessage")]
    public async Task ScoreAuditMessage2_ReturnsExpectedResponse(string endpoint)
    {
        // Arrange
        var piqiRequest = new PIQIRequest
        {
            DataProviderID = "TestProvider",
            DataSourceID = "TestSource",
            PIQIModelMnemonic = "PAT_CLINICAL_V1",
            EvaluationRubricMnemonic = "USCDI_V3",
            MessageID = "Msg002",
            MessageData = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData/Test2_PIQI.json"))
        };
        var result = new PIQIResponse();
        var requestContent = new StringContent(JsonConvert.SerializeObject(piqiRequest), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(endpoint, requestContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var contentType = response.Content.Headers.ContentType.MediaType;
        if (contentType == "text/plain" || contentType == "application/json")
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            result = JsonConvert.DeserializeObject<PIQIResponse>(responseBody);

            Assert.NotNull(result);
        }

        #region Check Results

        #region Overall Message Results
        Assert.Equal(result.Succeeded, true);
        Assert.Equal(result.ScoringData.MessageResults.PIQIScore, 44);
        Assert.Equal(result.ScoringData.MessageResults.Numerator, 4);
        Assert.Equal(result.ScoringData.MessageResults.Denominator, 9);
        Assert.Equal(result.ScoringData.MessageResults.CriticalFailureCount, 0);
        Assert.Equal(result.ScoringData.MessageResults.WeightedPIQIScore, 44);
        Assert.Equal(result.ScoringData.MessageResults.WeightedNumerator, 4);
        Assert.Equal(result.ScoringData.MessageResults.WeightedDenominator, 9);
        #endregion

        #region Data Class Results
        Assert.Equal(result.ScoringData.DataClassResults.Count, 15);

        #region Allergies
        var allergies = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Allergies");
        Assert.Equal(allergies.InstanceCount, 0);
        Assert.Equal(allergies.PIQIScore, 0);
        Assert.Equal(allergies.Numerator, 0);
        Assert.Equal(allergies.Denominator, 0);
        Assert.Equal(allergies.CriticalFailureCount, 0);
        Assert.Equal(allergies.WeightedPIQIScore, 0);
        Assert.Equal(allergies.WeightedNumerator, 0);
        Assert.Equal(allergies.WeightedDenominator, 0);
        #endregion

        #region Clinical Documents
        var clinicalDocuments = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Clinical Documents");
        Assert.Equal(clinicalDocuments.InstanceCount, 0);
        Assert.Equal(clinicalDocuments.PIQIScore, 0);
        Assert.Equal(clinicalDocuments.Numerator, 0);
        Assert.Equal(clinicalDocuments.Denominator, 0);
        Assert.Equal(clinicalDocuments.CriticalFailureCount, 0);
        Assert.Equal(clinicalDocuments.WeightedPIQIScore, 0);
        Assert.Equal(clinicalDocuments.WeightedNumerator, 0);
        Assert.Equal(clinicalDocuments.WeightedDenominator, 0);
        #endregion

        #region Conditions
        var conditions = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Conditions");
        Assert.Equal(conditions.InstanceCount, 0);
        Assert.Equal(conditions.PIQIScore, 0);
        Assert.Equal(conditions.Numerator, 0);
        Assert.Equal(conditions.Denominator, 0);
        Assert.Equal(conditions.CriticalFailureCount, 0);
        Assert.Equal(conditions.WeightedPIQIScore, 0);
        Assert.Equal(conditions.WeightedNumerator, 0);
        Assert.Equal(conditions.WeightedDenominator, 0);
        #endregion

        #region Demographics
        var demographics = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Demographics");
        Assert.Equal(demographics.InstanceCount, 1);
        Assert.Equal(demographics.PIQIScore, 75);
        Assert.Equal(demographics.Numerator, 3);
        Assert.Equal(demographics.Denominator, 4);
        Assert.Equal(demographics.CriticalFailureCount, 0);
        Assert.Equal(demographics.WeightedPIQIScore, 75);
        Assert.Equal(demographics.WeightedNumerator, 3);
        Assert.Equal(demographics.WeightedDenominator, 4);
        #endregion

        #region Diagnostic Imaging
        var diagnosticImaging = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Diagnostic Imaging");
        Assert.Equal(diagnosticImaging.InstanceCount, 0);
        Assert.Equal(diagnosticImaging.PIQIScore, 0);
        Assert.Equal(diagnosticImaging.Numerator, 0);
        Assert.Equal(diagnosticImaging.Denominator, 0);
        Assert.Equal(diagnosticImaging.CriticalFailureCount, 0);
        Assert.Equal(diagnosticImaging.WeightedPIQIScore, 0);
        Assert.Equal(diagnosticImaging.WeightedNumerator, 0);
        Assert.Equal(diagnosticImaging.WeightedDenominator, 0);
        #endregion

        #region Encounters
        var encounters = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Encounters");
        Assert.Equal(encounters.InstanceCount, 1);
        Assert.Equal(encounters.PIQIScore, 20);
        Assert.Equal(encounters.Numerator, 1);
        Assert.Equal(encounters.Denominator, 5);
        Assert.Equal(encounters.CriticalFailureCount, 0);
        Assert.Equal(encounters.WeightedPIQIScore, 20);
        Assert.Equal(encounters.WeightedNumerator, 1);
        Assert.Equal(encounters.WeightedDenominator, 5);
        #endregion

        #region Goals
        var goals = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Goals");
        Assert.Equal(goals.InstanceCount, 0);
        Assert.Equal(goals.PIQIScore, 0);
        Assert.Equal(goals.Numerator, 0);
        Assert.Equal(goals.Denominator, 0);
        Assert.Equal(goals.CriticalFailureCount, 0);
        Assert.Equal(goals.WeightedPIQIScore, 0);
        Assert.Equal(goals.WeightedNumerator, 0);
        Assert.Equal(goals.WeightedDenominator, 0);
        #endregion

        #region Health Assessments
        var healthAssessments = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Health Assessments");
        Assert.Equal(healthAssessments.InstanceCount, 0);
        Assert.Equal(healthAssessments.PIQIScore, 0);
        Assert.Equal(healthAssessments.Numerator, 0);
        Assert.Equal(healthAssessments.Denominator, 0);
        Assert.Equal(healthAssessments.CriticalFailureCount, 0);
        Assert.Equal(healthAssessments.WeightedPIQIScore, 0);
        Assert.Equal(healthAssessments.WeightedNumerator, 0);
        Assert.Equal(healthAssessments.WeightedDenominator, 0);
        #endregion

        #region Immunizations
        var immunizations = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Immunizations");
        Assert.Equal(immunizations.InstanceCount, 0);
        Assert.Equal(immunizations.PIQIScore, 0);
        Assert.Equal(immunizations.Numerator, 0);
        Assert.Equal(immunizations.Denominator, 0);
        Assert.Equal(immunizations.CriticalFailureCount, 0);
        Assert.Equal(immunizations.WeightedPIQIScore, 0);
        Assert.Equal(immunizations.WeightedNumerator, 0);
        Assert.Equal(immunizations.WeightedDenominator, 0);
        #endregion

        #region Lab Results
        var labResults = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Lab Results");
        Assert.Equal(labResults.InstanceCount, 0);
        Assert.Equal(labResults.PIQIScore, 0);
        Assert.Equal(labResults.Numerator, 0);
        Assert.Equal(labResults.Denominator, 0);
        Assert.Equal(labResults.CriticalFailureCount, 0);
        Assert.Equal(labResults.WeightedPIQIScore, 0);
        Assert.Equal(labResults.WeightedNumerator, 0);
        Assert.Equal(labResults.WeightedDenominator, 0);
        #endregion

        #region Medical Devices
        var medicalDevices = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Medical Devices");
        Assert.Equal(medicalDevices.InstanceCount, 0);
        Assert.Equal(medicalDevices.PIQIScore, 0);
        Assert.Equal(medicalDevices.Numerator, 0);
        Assert.Equal(medicalDevices.Denominator, 0);
        Assert.Equal(medicalDevices.CriticalFailureCount, 0);
        Assert.Equal(medicalDevices.WeightedPIQIScore, 0);
        Assert.Equal(medicalDevices.WeightedNumerator, 0);
        Assert.Equal(medicalDevices.WeightedDenominator, 0);
        #endregion

        #region Medications
        var medications = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Medications");
        Assert.Equal(medications.InstanceCount, 0);
        Assert.Equal(medications.PIQIScore, 0);
        Assert.Equal(medications.Numerator, 0);
        Assert.Equal(medications.Denominator, 0);
        Assert.Equal(medications.CriticalFailureCount, 0);
        Assert.Equal(medications.WeightedPIQIScore, 0);
        Assert.Equal(medications.WeightedNumerator, 0);
        Assert.Equal(medications.WeightedDenominator, 0);
        #endregion

        #region Procedures
        var procedures = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Procedures");
        Assert.Equal(procedures.InstanceCount, 0);
        Assert.Equal(procedures.PIQIScore, 0);
        Assert.Equal(procedures.Numerator, 0);
        Assert.Equal(procedures.Denominator, 0);
        Assert.Equal(procedures.CriticalFailureCount, 0);
        Assert.Equal(procedures.WeightedPIQIScore, 0);
        Assert.Equal(procedures.WeightedNumerator, 0);
        Assert.Equal(procedures.WeightedDenominator, 0);
        #endregion

        #region Vital Signs
        var vitalSigns = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Vital Signs");
        Assert.Equal(vitalSigns.InstanceCount, 0);
        Assert.Equal(vitalSigns.PIQIScore, 0);
        Assert.Equal(vitalSigns.Numerator, 0);
        Assert.Equal(vitalSigns.Denominator, 0);
        Assert.Equal(vitalSigns.CriticalFailureCount, 0);
        Assert.Equal(vitalSigns.WeightedPIQIScore, 0);
        Assert.Equal(vitalSigns.WeightedNumerator, 0);
        Assert.Equal(vitalSigns.WeightedDenominator, 0);
        #endregion

        #endregion

        #region Informational Results
        Assert.Equal(result.ScoringData.InformationalResults.Count, 15);

        #region Allergies
        var allergiesInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Allergies");
        Assert.Equal(allergiesInfo.EvaluationList.Count, 0);
        #endregion

        #region Clinical Documents
        var clinicalDocumentsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Clinical Documents");
        Assert.Equal(clinicalDocumentsInfo.EvaluationList.Count, 0);
        #endregion

        #region Conditions
        var conditionsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Conditions");
        Assert.Equal(conditionsInfo.EvaluationList.Count, 0);
        #endregion

        #region Demographics
        var demographicsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Demographics");
        Assert.Equal(demographicsInfo.EvaluationList.Count, 0);
        #endregion

        #region Diagnostic Imaging
        var diagnosticImagingInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Diagnostic Imaging");
        Assert.Equal(diagnosticImagingInfo.EvaluationList.Count, 0);
        #endregion

        #region Encounters
        var encountersInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Encounters");
        Assert.Equal(encountersInfo.EvaluationList.Count, 0);
        #endregion

        #region Goals
        var goalsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Goals");
        Assert.Equal(goalsInfo.EvaluationList.Count, 0);
        #endregion

        #region Health Assessments
        var healthAssessmentsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Health Assessments");
        Assert.Equal(healthAssessmentsInfo.EvaluationList.Count, 0);
        #endregion

        #region Immunizations
        var immunizationsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Immunizations");
        Assert.Equal(immunizationsInfo.EvaluationList.Count, 0);
        #endregion

        #region Lab Results
        var labResultsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Lab Results");
        Assert.Equal(labResultsInfo.EvaluationList.Count, 0);
        #endregion

        #region Medical Devices
        var medicalDevicesInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Medical Devices");
        Assert.Equal(medicalDevicesInfo.EvaluationList.Count, 0);
        #endregion

        #region Medications
        var medicationsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Medications");
        Assert.Equal(medicationsInfo.EvaluationList.Count, 0);
        #endregion

        #region Procedures
        var proceduresInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Procedures");
        Assert.Equal(proceduresInfo.EvaluationList.Count, 0);
        #endregion

        #region Vital Signs
        var vitalSignsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Vital Signs");
        Assert.Equal(vitalSignsInfo.EvaluationList.Count, 0);
        #endregion

        #endregion

        #region Audit Results
        Assert.Equal(Regex.Replace(result.AuditedMessage, @"\s+", ""), Regex.Replace("{\r\n  \"EntityModelMnemonic\": \"PAT_CLINICAL_V1\",\r\n  \"DataProviderID\": \"TestProvider\",\r\n  \"DataSourceID\": \"TestSource\",\r\n  \"MessageID\": \"Msg002\",\r\n  \"Audit\": {\r\n    \"messageNumerator\": \"4\",\r\n    \"messageDenominator\": \"9\",\r\n    \"messageScore\": \"44\",\r\n    \"messageNumeratorWeighted\": \"4\",\r\n    \"messageDenominatorWeighted\": \"9\",\r\n    \"messageScoreWeighted\": \"44\",\r\n    \"messageCriticalFailureCount\": \"0\"\r\n  },\r\n  \"patient\": {\r\n    \"allergies\": [],\r\n    \"clinicalDocuments\": [],\r\n    \"conditions\": [],\r\n    \"demographics\": [\r\n      {\r\n        \"birthDate\": {\r\n          \"data\": \"2015-01-01\",\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"100\",\r\n              \"attributeScoreWeighted\": \"100\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"1\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"DEM_DOB\",\r\n                \"attributeName\": \"birthDate\",\r\n                \"assessment\": \"Date of birth is valid past date\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Passed\",\r\n                \"reason\": \"\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"birthSex\": {\r\n          \"data\": {\r\n            \"codings\": [\r\n              {\r\n                \"system\": \"http://hl7.org/fhir/administrative-gender\",\r\n                \"code\": \"unknown\",\r\n                \"display\": \"unknown\"\r\n              }\r\n            ],\r\n            \"text\": \"unknown\"\r\n          },\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"DEM_SEX\",\r\n                \"attributeName\": \"birthSex\",\r\n                \"assessment\": \"Birth sex is SNOMED-CT\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"invalid concept\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"deathDate\": {},\r\n        \"deceased\": {},\r\n        \"ethnicity\": {\r\n          \"data\": {\r\n            \"codings\": [\r\n              {\r\n                \"system\": \"urn:oid:2.16.840.1.113883.6.238\",\r\n                \"code\": \"2135-2\",\r\n                \"display\": \"Hispanic or Latino\"\r\n              }\r\n            ],\r\n            \"text\": \"Hispanic or Latino\"\r\n          },\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"100\",\r\n              \"attributeScoreWeighted\": \"100\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"1\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"DEM_ETHN\",\r\n                \"attributeName\": \"ethnicity\",\r\n                \"assessment\": \"Ethnicity is valid code\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Passed\",\r\n                \"reason\": \"\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"genderIdentity\": {},\r\n        \"maritalStatus\": {},\r\n        \"patientIdentifier\": {},\r\n        \"primaryLanguage\": {},\r\n        \"race\": {\r\n          \"data\": {\r\n            \"codings\": [\r\n              {\r\n                \"system\": \"urn:oid:2.16.840.1.113883.6.238\",\r\n                \"code\": \"1002-5\",\r\n                \"display\": \"American Indian or Alaska Native\"\r\n              }\r\n            ],\r\n            \"text\": \"American Indian or Alaska Native\"\r\n          },\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"100\",\r\n              \"attributeScoreWeighted\": \"100\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"1\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"DEM_RACE\",\r\n                \"attributeName\": \"race\",\r\n                \"assessment\": \"Race is valid concept\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Passed\",\r\n                \"reason\": \"\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"elementAudit\": {\r\n          \"elementScore\": \"75\",\r\n          \"elementScoreWeighted\": \"75\",\r\n          \"elementCriticalFailureCount\": \"0\",\r\n          \"elementNumerator\": \"3\",\r\n          \"elementDenominator\": \"4\"\r\n        }\r\n      }\r\n    ],\r\n    \"diagnosticImaging\": [],\r\n    \"encounters\": [\r\n      {\r\n        \"encounterDateTime\": {\r\n          \"data\": \"8/12/2026 4:00:00 AM\",\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"ENC_DATETIME\",\r\n                \"attributeName\": \"encounter date/time\",\r\n                \"assessment\": \"Encounter date is in the past\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"Encounter date is in not the past\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"encounterDiagnosis\": {\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"ENC_DIAGNOSIS\",\r\n                \"attributeName\": \"encounter diagnosis\",\r\n                \"assessment\": \"Diagnosis is SNOMED-CT or ICD-10-CM\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"unpopulated\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"encounterDisposition\": {\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"ENC_DISPOSITION\",\r\n                \"attributeName\": \"encounter disposition\",\r\n                \"assessment\": \"Encounter disposition is populated\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"Encounter disposition is not populated\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"encounterEndDateTime\": {},\r\n        \"encounterIdentifier\": {},\r\n        \"encounterLocation\": {\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"ENC_LOCATION\",\r\n                \"attributeName\": \"encounter location\",\r\n                \"assessment\": \"Encounter location is populated\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"Encounter location is not populated\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"encounterReason\": {},\r\n        \"encounterStatus\": {},\r\n        \"encounterType\": {\r\n          \"data\": {\r\n            \"text\": \"Psychiatric interview and evaluation (procedure)\"\r\n          },\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"100\",\r\n              \"attributeScoreWeighted\": \"100\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"1\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"ENC_TYPE\",\r\n                \"attributeName\": \"encounter type\",\r\n                \"assessment\": \"Encounter type is populated\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Passed\",\r\n                \"reason\": \"\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"elementAudit\": {\r\n          \"elementScore\": \"20\",\r\n          \"elementScoreWeighted\": \"20\",\r\n          \"elementCriticalFailureCount\": \"0\",\r\n          \"elementNumerator\": \"1\",\r\n          \"elementDenominator\": \"5\"\r\n        }\r\n      }\r\n    ],\r\n    \"goals\": [],\r\n    \"healthAssessments\": [],\r\n    \"immunizations\": [],\r\n    \"labResults\": [],\r\n    \"medicalDevices\": [],\r\n    \"medications\": [],\r\n    \"procedures\": [],\r\n    \"providers\": [],\r\n    \"vitalSigns\": []\r\n  }\r\n}", @"\s+", ""));
        #endregion

        #endregion
    }

    [Theory]
    [InlineData("/PIQI/ScoreMessage")]
    public async Task ScoresMessage3_ReturnsExpectedResponse(string endpoint)
    {
        // Arrange
        var piqiRequest = new PIQIRequest
        {
            DataProviderID = "TestProvider",
            DataSourceID = "TestSource",
            PIQIModelMnemonic = "PAT_CLINICAL_V1",
            EvaluationRubricMnemonic = "USCDI_V3",
            MessageID = "Msg003",
            MessageData = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData/Test3_PIQI.json"))
        };
        var result = new PIQIResponse();
        var requestContent = new StringContent(JsonConvert.SerializeObject(piqiRequest), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(endpoint, requestContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var contentType = response.Content.Headers.ContentType.MediaType;
        if (contentType == "text/plain" || contentType == "application/json")
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            result = JsonConvert.DeserializeObject<PIQIResponse>(responseBody);

            Assert.NotNull(result);
        }

        #region Check Results

        #region Overall Message Results
        Assert.Equal(result.Succeeded, true);
        Assert.Equal(result.ScoringData.MessageResults.PIQIScore, 42);
        Assert.Equal(result.ScoringData.MessageResults.Numerator, 6);
        Assert.Equal(result.ScoringData.MessageResults.Denominator, 14);
        Assert.Equal(result.ScoringData.MessageResults.CriticalFailureCount, 0);
        Assert.Equal(result.ScoringData.MessageResults.WeightedPIQIScore, 42);
        Assert.Equal(result.ScoringData.MessageResults.WeightedNumerator, 6);
        Assert.Equal(result.ScoringData.MessageResults.WeightedDenominator, 14);
        #endregion

        #region Data Class Results
        Assert.Equal(result.ScoringData.DataClassResults.Count, 15);

        #region Allergies
        var allergies = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Allergies");
        Assert.Equal(allergies.InstanceCount, 0);
        Assert.Equal(allergies.PIQIScore, 0);
        Assert.Equal(allergies.Numerator, 0);
        Assert.Equal(allergies.Denominator, 0);
        Assert.Equal(allergies.CriticalFailureCount, 0);
        Assert.Equal(allergies.WeightedPIQIScore, 0);
        Assert.Equal(allergies.WeightedNumerator, 0);
        Assert.Equal(allergies.WeightedDenominator, 0);
        #endregion

        #region Clinical Documents
        var clinicalDocuments = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Clinical Documents");
        Assert.Equal(clinicalDocuments.InstanceCount, 0);
        Assert.Equal(clinicalDocuments.PIQIScore, 0);
        Assert.Equal(clinicalDocuments.Numerator, 0);
        Assert.Equal(clinicalDocuments.Denominator, 0);
        Assert.Equal(clinicalDocuments.CriticalFailureCount, 0);
        Assert.Equal(clinicalDocuments.WeightedPIQIScore, 0);
        Assert.Equal(clinicalDocuments.WeightedNumerator, 0);
        Assert.Equal(clinicalDocuments.WeightedDenominator, 0);
        #endregion

        #region Conditions
        var conditions = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Conditions");
        Assert.Equal(conditions.InstanceCount, 0);
        Assert.Equal(conditions.PIQIScore, 0);
        Assert.Equal(conditions.Numerator, 0);
        Assert.Equal(conditions.Denominator, 0);
        Assert.Equal(conditions.CriticalFailureCount, 0);
        Assert.Equal(conditions.WeightedPIQIScore, 0);
        Assert.Equal(conditions.WeightedNumerator, 0);
        Assert.Equal(conditions.WeightedDenominator, 0);
        #endregion

        #region Demographics
        var demographics = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Demographics");
        Assert.Equal(demographics.InstanceCount, 1);
        Assert.Equal(demographics.PIQIScore, 75);
        Assert.Equal(demographics.Numerator, 3);
        Assert.Equal(demographics.Denominator, 4);
        Assert.Equal(demographics.CriticalFailureCount, 0);
        Assert.Equal(demographics.WeightedPIQIScore, 75);
        Assert.Equal(demographics.WeightedNumerator, 3);
        Assert.Equal(demographics.WeightedDenominator, 4);
        #endregion

        #region Diagnostic Imaging
        var diagnosticImaging = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Diagnostic Imaging");
        Assert.Equal(diagnosticImaging.InstanceCount, 0);
        Assert.Equal(diagnosticImaging.PIQIScore, 0);
        Assert.Equal(diagnosticImaging.Numerator, 0);
        Assert.Equal(diagnosticImaging.Denominator, 0);
        Assert.Equal(diagnosticImaging.CriticalFailureCount, 0);
        Assert.Equal(diagnosticImaging.WeightedPIQIScore, 0);
        Assert.Equal(diagnosticImaging.WeightedNumerator, 0);
        Assert.Equal(diagnosticImaging.WeightedDenominator, 0);
        #endregion

        #region Encounters
        var encounters = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Encounters");
        Assert.Equal(encounters.InstanceCount, 1);
        Assert.Equal(encounters.PIQIScore, 20);
        Assert.Equal(encounters.Numerator, 1);
        Assert.Equal(encounters.Denominator, 5);
        Assert.Equal(encounters.CriticalFailureCount, 0);
        Assert.Equal(encounters.WeightedPIQIScore, 20);
        Assert.Equal(encounters.WeightedNumerator, 1);
        Assert.Equal(encounters.WeightedDenominator, 5);
        #endregion

        #region Goals
        var goals = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Goals");
        Assert.Equal(goals.InstanceCount, 0);
        Assert.Equal(goals.PIQIScore, 0);
        Assert.Equal(goals.Numerator, 0);
        Assert.Equal(goals.Denominator, 0);
        Assert.Equal(goals.CriticalFailureCount, 0);
        Assert.Equal(goals.WeightedPIQIScore, 0);
        Assert.Equal(goals.WeightedNumerator, 0);
        Assert.Equal(goals.WeightedDenominator, 0);
        #endregion

        #region Health Assessments
        var healthAssessments = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Health Assessments");
        Assert.Equal(healthAssessments.InstanceCount, 1);
        Assert.Equal(healthAssessments.PIQIScore, 50);
        Assert.Equal(healthAssessments.Numerator, 1);
        Assert.Equal(healthAssessments.Denominator, 2);
        Assert.Equal(healthAssessments.CriticalFailureCount, 0);
        Assert.Equal(healthAssessments.WeightedPIQIScore, 50);
        Assert.Equal(healthAssessments.WeightedNumerator, 1);
        Assert.Equal(healthAssessments.WeightedDenominator, 2);
        #endregion

        #region Immunizations
        var immunizations = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Immunizations");
        Assert.Equal(immunizations.InstanceCount, 0);
        Assert.Equal(immunizations.PIQIScore, 0);
        Assert.Equal(immunizations.Numerator, 0);
        Assert.Equal(immunizations.Denominator, 0);
        Assert.Equal(immunizations.CriticalFailureCount, 0);
        Assert.Equal(immunizations.WeightedPIQIScore, 0);
        Assert.Equal(immunizations.WeightedNumerator, 0);
        Assert.Equal(immunizations.WeightedDenominator, 0);
        #endregion

        #region Lab Results
        var labResults = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Lab Results");
        Assert.Equal(labResults.InstanceCount, 0);
        Assert.Equal(labResults.PIQIScore, 0);
        Assert.Equal(labResults.Numerator, 0);
        Assert.Equal(labResults.Denominator, 0);
        Assert.Equal(labResults.CriticalFailureCount, 0);
        Assert.Equal(labResults.WeightedPIQIScore, 0);
        Assert.Equal(labResults.WeightedNumerator, 0);
        Assert.Equal(labResults.WeightedDenominator, 0);
        #endregion

        #region Medical Devices
        var medicalDevices = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Medical Devices");
        Assert.Equal(medicalDevices.InstanceCount, 0);
        Assert.Equal(medicalDevices.PIQIScore, 0);
        Assert.Equal(medicalDevices.Numerator, 0);
        Assert.Equal(medicalDevices.Denominator, 0);
        Assert.Equal(medicalDevices.CriticalFailureCount, 0);
        Assert.Equal(medicalDevices.WeightedPIQIScore, 0);
        Assert.Equal(medicalDevices.WeightedNumerator, 0);
        Assert.Equal(medicalDevices.WeightedDenominator, 0);
        #endregion

        #region Medications
        var medications = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Medications");
        Assert.Equal(medications.InstanceCount, 0);
        Assert.Equal(medications.PIQIScore, 0);
        Assert.Equal(medications.Numerator, 0);
        Assert.Equal(medications.Denominator, 0);
        Assert.Equal(medications.CriticalFailureCount, 0);
        Assert.Equal(medications.WeightedPIQIScore, 0);
        Assert.Equal(medications.WeightedNumerator, 0);
        Assert.Equal(medications.WeightedDenominator, 0);
        #endregion

        #region Procedures
        var procedures = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Procedures");
        Assert.Equal(procedures.InstanceCount, 1);
        Assert.Equal(procedures.PIQIScore, 33);
        Assert.Equal(procedures.Numerator, 1);
        Assert.Equal(procedures.Denominator, 3);
        Assert.Equal(procedures.CriticalFailureCount, 0);
        Assert.Equal(procedures.WeightedPIQIScore, 33);
        Assert.Equal(procedures.WeightedNumerator, 1);
        Assert.Equal(procedures.WeightedDenominator, 3);
        #endregion

        #region Vital Signs
        var vitalSigns = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Vital Signs");
        Assert.Equal(vitalSigns.InstanceCount, 0);
        Assert.Equal(vitalSigns.PIQIScore, 0);
        Assert.Equal(vitalSigns.Numerator, 0);
        Assert.Equal(vitalSigns.Denominator, 0);
        Assert.Equal(vitalSigns.CriticalFailureCount, 0);
        Assert.Equal(vitalSigns.WeightedPIQIScore, 0);
        Assert.Equal(vitalSigns.WeightedNumerator, 0);
        Assert.Equal(vitalSigns.WeightedDenominator, 0);
        #endregion

        #endregion

        #region Informational Results
        Assert.Equal(result.ScoringData.InformationalResults.Count, 15);

        #region Allergies
        var allergiesInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Allergies");
        Assert.Equal(allergiesInfo.EvaluationList.Count, 0);
        #endregion

        #region Clinical Documents
        var clinicalDocumentsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Clinical Documents");
        Assert.Equal(clinicalDocumentsInfo.EvaluationList.Count, 0);
        #endregion

        #region Conditions
        var conditionsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Conditions");
        Assert.Equal(conditionsInfo.EvaluationList.Count, 0);
        #endregion

        #region Demographics
        var demographicsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Demographics");
        Assert.Equal(demographicsInfo.EvaluationList.Count, 0);
        #endregion

        #region Diagnostic Imaging
        var diagnosticImagingInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Diagnostic Imaging");
        Assert.Equal(diagnosticImagingInfo.EvaluationList.Count, 0);
        #endregion

        #region Encounters
        var encountersInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Encounters");
        Assert.Equal(encountersInfo.EvaluationList.Count, 0);
        #endregion

        #region Goals
        var goalsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Goals");
        Assert.Equal(goalsInfo.EvaluationList.Count, 0);
        #endregion

        #region Health Assessments
        var healthAssessmentsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Health Assessments");
        Assert.Equal(healthAssessmentsInfo.EvaluationList.Count, 0);
        #endregion

        #region Immunizations
        var immunizationsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Immunizations");
        Assert.Equal(immunizationsInfo.EvaluationList.Count, 0);
        #endregion

        #region Lab Results
        var labResultsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Lab Results");
        Assert.Equal(labResultsInfo.EvaluationList.Count, 0);
        #endregion

        #region Medical Devices
        var medicalDevicesInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Medical Devices");
        Assert.Equal(medicalDevicesInfo.EvaluationList.Count, 0);
        #endregion

        #region Medications
        var medicationsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Medications");
        Assert.Equal(medicationsInfo.EvaluationList.Count, 0);
        #endregion

        #region Procedures
        var proceduresInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Procedures");
        Assert.Equal(proceduresInfo.EvaluationList.Count, 0);
        #endregion

        #region Vital Signs
        var vitalSignsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Vital Signs");
        Assert.Equal(vitalSignsInfo.EvaluationList.Count, 0);
        #endregion

        #endregion

        #endregion
    }

    [Theory]
    [InlineData("/PIQI/ScoreAuditMessage")]
    public async Task ScoreAuditMessage3_ReturnsExpectedResponse(string endpoint)
    {
        // Arrange
        var piqiRequest = new PIQIRequest
        {
            DataProviderID = "TestProvider",
            DataSourceID = "TestSource",
            PIQIModelMnemonic = "PAT_CLINICAL_V1",
            EvaluationRubricMnemonic = "USCDI_V3",
            MessageID = "Msg003",
            MessageData = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData/Test3_PIQI.json"))
        };
        var result = new PIQIResponse();
        var requestContent = new StringContent(JsonConvert.SerializeObject(piqiRequest), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(endpoint, requestContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var contentType = response.Content.Headers.ContentType.MediaType;
        if (contentType == "text/plain" || contentType == "application/json")
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            result = JsonConvert.DeserializeObject<PIQIResponse>(responseBody);

            Assert.NotNull(result);
        }

        #region Check Results

        #region Overall Message Results
        Assert.Equal(result.Succeeded, true);
        Assert.Equal(result.ScoringData.MessageResults.PIQIScore, 42);
        Assert.Equal(result.ScoringData.MessageResults.Numerator, 6);
        Assert.Equal(result.ScoringData.MessageResults.Denominator, 14);
        Assert.Equal(result.ScoringData.MessageResults.CriticalFailureCount, 0);
        Assert.Equal(result.ScoringData.MessageResults.WeightedPIQIScore, 42);
        Assert.Equal(result.ScoringData.MessageResults.WeightedNumerator, 6);
        Assert.Equal(result.ScoringData.MessageResults.WeightedDenominator, 14);
        #endregion

        #region Data Class Results
        Assert.Equal(result.ScoringData.DataClassResults.Count, 15);

        #region Allergies
        var allergies = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Allergies");
        Assert.Equal(allergies.InstanceCount, 0);
        Assert.Equal(allergies.PIQIScore, 0);
        Assert.Equal(allergies.Numerator, 0);
        Assert.Equal(allergies.Denominator, 0);
        Assert.Equal(allergies.CriticalFailureCount, 0);
        Assert.Equal(allergies.WeightedPIQIScore, 0);
        Assert.Equal(allergies.WeightedNumerator, 0);
        Assert.Equal(allergies.WeightedDenominator, 0);
        #endregion

        #region Clinical Documents
        var clinicalDocuments = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Clinical Documents");
        Assert.Equal(clinicalDocuments.InstanceCount, 0);
        Assert.Equal(clinicalDocuments.PIQIScore, 0);
        Assert.Equal(clinicalDocuments.Numerator, 0);
        Assert.Equal(clinicalDocuments.Denominator, 0);
        Assert.Equal(clinicalDocuments.CriticalFailureCount, 0);
        Assert.Equal(clinicalDocuments.WeightedPIQIScore, 0);
        Assert.Equal(clinicalDocuments.WeightedNumerator, 0);
        Assert.Equal(clinicalDocuments.WeightedDenominator, 0);
        #endregion

        #region Conditions
        var conditions = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Conditions");
        Assert.Equal(conditions.InstanceCount, 0);
        Assert.Equal(conditions.PIQIScore, 0);
        Assert.Equal(conditions.Numerator, 0);
        Assert.Equal(conditions.Denominator, 0);
        Assert.Equal(conditions.CriticalFailureCount, 0);
        Assert.Equal(conditions.WeightedPIQIScore, 0);
        Assert.Equal(conditions.WeightedNumerator, 0);
        Assert.Equal(conditions.WeightedDenominator, 0);
        #endregion

        #region Demographics
        var demographics = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Demographics");
        Assert.Equal(demographics.InstanceCount, 1);
        Assert.Equal(demographics.PIQIScore, 75);
        Assert.Equal(demographics.Numerator, 3);
        Assert.Equal(demographics.Denominator, 4);
        Assert.Equal(demographics.CriticalFailureCount, 0);
        Assert.Equal(demographics.WeightedPIQIScore, 75);
        Assert.Equal(demographics.WeightedNumerator, 3);
        Assert.Equal(demographics.WeightedDenominator, 4);
        #endregion

        #region Diagnostic Imaging
        var diagnosticImaging = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Diagnostic Imaging");
        Assert.Equal(diagnosticImaging.InstanceCount, 0);
        Assert.Equal(diagnosticImaging.PIQIScore, 0);
        Assert.Equal(diagnosticImaging.Numerator, 0);
        Assert.Equal(diagnosticImaging.Denominator, 0);
        Assert.Equal(diagnosticImaging.CriticalFailureCount, 0);
        Assert.Equal(diagnosticImaging.WeightedPIQIScore, 0);
        Assert.Equal(diagnosticImaging.WeightedNumerator, 0);
        Assert.Equal(diagnosticImaging.WeightedDenominator, 0);
        #endregion

        #region Encounters
        var encounters = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Encounters");
        Assert.Equal(encounters.InstanceCount, 1);
        Assert.Equal(encounters.PIQIScore, 20);
        Assert.Equal(encounters.Numerator, 1);
        Assert.Equal(encounters.Denominator, 5);
        Assert.Equal(encounters.CriticalFailureCount, 0);
        Assert.Equal(encounters.WeightedPIQIScore, 20);
        Assert.Equal(encounters.WeightedNumerator, 1);
        Assert.Equal(encounters.WeightedDenominator, 5);
        #endregion

        #region Goals
        var goals = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Goals");
        Assert.Equal(goals.InstanceCount, 0);
        Assert.Equal(goals.PIQIScore, 0);
        Assert.Equal(goals.Numerator, 0);
        Assert.Equal(goals.Denominator, 0);
        Assert.Equal(goals.CriticalFailureCount, 0);
        Assert.Equal(goals.WeightedPIQIScore, 0);
        Assert.Equal(goals.WeightedNumerator, 0);
        Assert.Equal(goals.WeightedDenominator, 0);
        #endregion

        #region Health Assessments
        var healthAssessments = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Health Assessments");
        Assert.Equal(healthAssessments.InstanceCount, 1);
        Assert.Equal(healthAssessments.PIQIScore, 50);
        Assert.Equal(healthAssessments.Numerator, 1);
        Assert.Equal(healthAssessments.Denominator, 2);
        Assert.Equal(healthAssessments.CriticalFailureCount, 0);
        Assert.Equal(healthAssessments.WeightedPIQIScore, 50);
        Assert.Equal(healthAssessments.WeightedNumerator, 1);
        Assert.Equal(healthAssessments.WeightedDenominator, 2);
        #endregion

        #region Immunizations
        var immunizations = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Immunizations");
        Assert.Equal(immunizations.InstanceCount, 0);
        Assert.Equal(immunizations.PIQIScore, 0);
        Assert.Equal(immunizations.Numerator, 0);
        Assert.Equal(immunizations.Denominator, 0);
        Assert.Equal(immunizations.CriticalFailureCount, 0);
        Assert.Equal(immunizations.WeightedPIQIScore, 0);
        Assert.Equal(immunizations.WeightedNumerator, 0);
        Assert.Equal(immunizations.WeightedDenominator, 0);
        #endregion

        #region Lab Results
        var labResults = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Lab Results");
        Assert.Equal(labResults.InstanceCount, 0);
        Assert.Equal(labResults.PIQIScore, 0);
        Assert.Equal(labResults.Numerator, 0);
        Assert.Equal(labResults.Denominator, 0);
        Assert.Equal(labResults.CriticalFailureCount, 0);
        Assert.Equal(labResults.WeightedPIQIScore, 0);
        Assert.Equal(labResults.WeightedNumerator, 0);
        Assert.Equal(labResults.WeightedDenominator, 0);
        #endregion

        #region Medical Devices
        var medicalDevices = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Medical Devices");
        Assert.Equal(medicalDevices.InstanceCount, 0);
        Assert.Equal(medicalDevices.PIQIScore, 0);
        Assert.Equal(medicalDevices.Numerator, 0);
        Assert.Equal(medicalDevices.Denominator, 0);
        Assert.Equal(medicalDevices.CriticalFailureCount, 0);
        Assert.Equal(medicalDevices.WeightedPIQIScore, 0);
        Assert.Equal(medicalDevices.WeightedNumerator, 0);
        Assert.Equal(medicalDevices.WeightedDenominator, 0);
        #endregion

        #region Medications
        var medications = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Medications");
        Assert.Equal(medications.InstanceCount, 0);
        Assert.Equal(medications.PIQIScore, 0);
        Assert.Equal(medications.Numerator, 0);
        Assert.Equal(medications.Denominator, 0);
        Assert.Equal(medications.CriticalFailureCount, 0);
        Assert.Equal(medications.WeightedPIQIScore, 0);
        Assert.Equal(medications.WeightedNumerator, 0);
        Assert.Equal(medications.WeightedDenominator, 0);
        #endregion

        #region Procedures
        var procedures = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Procedures");
        Assert.Equal(procedures.InstanceCount, 1);
        Assert.Equal(procedures.PIQIScore, 33);
        Assert.Equal(procedures.Numerator, 1);
        Assert.Equal(procedures.Denominator, 3);
        Assert.Equal(procedures.CriticalFailureCount, 0);
        Assert.Equal(procedures.WeightedPIQIScore, 33);
        Assert.Equal(procedures.WeightedNumerator, 1);
        Assert.Equal(procedures.WeightedDenominator, 3);
        #endregion

        #region Vital Signs
        var vitalSigns = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Vital Signs");
        Assert.Equal(vitalSigns.InstanceCount, 0);
        Assert.Equal(vitalSigns.PIQIScore, 0);
        Assert.Equal(vitalSigns.Numerator, 0);
        Assert.Equal(vitalSigns.Denominator, 0);
        Assert.Equal(vitalSigns.CriticalFailureCount, 0);
        Assert.Equal(vitalSigns.WeightedPIQIScore, 0);
        Assert.Equal(vitalSigns.WeightedNumerator, 0);
        Assert.Equal(vitalSigns.WeightedDenominator, 0);
        #endregion

        #endregion

        #region Informational Results
        Assert.Equal(result.ScoringData.InformationalResults.Count, 15);

        #region Allergies
        var allergiesInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Allergies");
        Assert.Equal(allergiesInfo.EvaluationList.Count, 0);
        #endregion

        #region Clinical Documents
        var clinicalDocumentsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Clinical Documents");
        Assert.Equal(clinicalDocumentsInfo.EvaluationList.Count, 0);
        #endregion

        #region Conditions
        var conditionsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Conditions");
        Assert.Equal(conditionsInfo.EvaluationList.Count, 0);
        #endregion

        #region Demographics
        var demographicsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Demographics");
        Assert.Equal(demographicsInfo.EvaluationList.Count, 0);
        #endregion

        #region Diagnostic Imaging
        var diagnosticImagingInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Diagnostic Imaging");
        Assert.Equal(diagnosticImagingInfo.EvaluationList.Count, 0);
        #endregion

        #region Encounters
        var encountersInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Encounters");
        Assert.Equal(encountersInfo.EvaluationList.Count, 0);
        #endregion

        #region Goals
        var goalsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Goals");
        Assert.Equal(goalsInfo.EvaluationList.Count, 0);
        #endregion

        #region Health Assessments
        var healthAssessmentsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Health Assessments");
        Assert.Equal(healthAssessmentsInfo.EvaluationList.Count, 0);
        #endregion

        #region Immunizations
        var immunizationsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Immunizations");
        Assert.Equal(immunizationsInfo.EvaluationList.Count, 0);
        #endregion

        #region Lab Results
        var labResultsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Lab Results");
        Assert.Equal(labResultsInfo.EvaluationList.Count, 0);
        #endregion

        #region Medical Devices
        var medicalDevicesInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Medical Devices");
        Assert.Equal(medicalDevicesInfo.EvaluationList.Count, 0);
        #endregion

        #region Medications
        var medicationsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Medications");
        Assert.Equal(medicationsInfo.EvaluationList.Count, 0);
        #endregion

        #region Procedures
        var proceduresInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Procedures");
        Assert.Equal(proceduresInfo.EvaluationList.Count, 0);
        #endregion

        #region Vital Signs
        var vitalSignsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Vital Signs");
        Assert.Equal(vitalSignsInfo.EvaluationList.Count, 0);
        #endregion

        #endregion

        #region Audit Results
        Assert.Equal(Regex.Replace(result.AuditedMessage, @"\s+", ""), Regex.Replace("{\r\n  \"EntityModelMnemonic\": \"PAT_CLINICAL_V1\",\r\n  \"DataProviderID\": \"TestProvider\",\r\n  \"DataSourceID\": \"TestSource\",\r\n  \"MessageID\": \"Msg003\",\r\n  \"Audit\": {\r\n    \"messageNumerator\": \"6\",\r\n    \"messageDenominator\": \"14\",\r\n    \"messageScore\": \"42\",\r\n    \"messageNumeratorWeighted\": \"6\",\r\n    \"messageDenominatorWeighted\": \"14\",\r\n    \"messageScoreWeighted\": \"42\",\r\n    \"messageCriticalFailureCount\": \"0\"\r\n  },\r\n  \"patient\": {\r\n    \"allergies\": [],\r\n    \"clinicalDocuments\": [],\r\n    \"conditions\": [],\r\n    \"demographics\": [\r\n      {\r\n        \"birthDate\": {\r\n          \"data\": \"2014-01-01\",\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"100\",\r\n              \"attributeScoreWeighted\": \"100\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"1\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"DEM_DOB\",\r\n                \"attributeName\": \"birthDate\",\r\n                \"assessment\": \"Date of birth is valid past date\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Passed\",\r\n                \"reason\": \"\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"birthSex\": {\r\n          \"data\": {\r\n            \"codings\": [\r\n              {\r\n                \"system\": \"http://hl7.org/fhir/administrative-gender\",\r\n                \"code\": \"male\",\r\n                \"display\": \"male\"\r\n              }\r\n            ],\r\n            \"text\": \"male\"\r\n          },\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"DEM_SEX\",\r\n                \"attributeName\": \"birthSex\",\r\n                \"assessment\": \"Birth sex is SNOMED-CT\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"invalid concept\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"deathDate\": {},\r\n        \"deceased\": {},\r\n        \"ethnicity\": {\r\n          \"data\": {\r\n            \"codings\": [\r\n              {\r\n                \"system\": \"urn:oid:2.16.840.1.113883.6.238\",\r\n                \"code\": \"2135-2\",\r\n                \"display\": \"Hispanic or Latino\"\r\n              }\r\n            ],\r\n            \"text\": \"Hispanic or Latino\"\r\n          },\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"100\",\r\n              \"attributeScoreWeighted\": \"100\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"1\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"DEM_ETHN\",\r\n                \"attributeName\": \"ethnicity\",\r\n                \"assessment\": \"Ethnicity is valid code\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Passed\",\r\n                \"reason\": \"\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"genderIdentity\": {},\r\n        \"maritalStatus\": {},\r\n        \"patientIdentifier\": {},\r\n        \"primaryLanguage\": {},\r\n        \"race\": {\r\n          \"data\": {\r\n            \"codings\": [\r\n              {\r\n                \"system\": \"urn:oid:2.16.840.1.113883.6.238\",\r\n                \"code\": \"1002-5\",\r\n                \"display\": \"American Indian or Alaska Native\"\r\n              }\r\n            ],\r\n            \"text\": \"American Indian or Alaska Native\"\r\n          },\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"100\",\r\n              \"attributeScoreWeighted\": \"100\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"1\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"DEM_RACE\",\r\n                \"attributeName\": \"race\",\r\n                \"assessment\": \"Race is valid concept\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Passed\",\r\n                \"reason\": \"\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"elementAudit\": {\r\n          \"elementScore\": \"75\",\r\n          \"elementScoreWeighted\": \"75\",\r\n          \"elementCriticalFailureCount\": \"0\",\r\n          \"elementNumerator\": \"3\",\r\n          \"elementDenominator\": \"4\"\r\n        }\r\n      }\r\n    ],\r\n    \"diagnosticImaging\": [],\r\n    \"encounters\": [\r\n      {\r\n        \"encounterDateTime\": {\r\n          \"data\": \"6/21/2026 4:00:00 AM\",\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"ENC_DATETIME\",\r\n                \"attributeName\": \"encounter date/time\",\r\n                \"assessment\": \"Encounter date is in the past\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"Encounter date is in not the past\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"encounterDiagnosis\": {\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"ENC_DIAGNOSIS\",\r\n                \"attributeName\": \"encounter diagnosis\",\r\n                \"assessment\": \"Diagnosis is SNOMED-CT or ICD-10-CM\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"unpopulated\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"encounterDisposition\": {\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"ENC_DISPOSITION\",\r\n                \"attributeName\": \"encounter disposition\",\r\n                \"assessment\": \"Encounter disposition is populated\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"Encounter disposition is not populated\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"encounterEndDateTime\": {},\r\n        \"encounterIdentifier\": {},\r\n        \"encounterLocation\": {\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"ENC_LOCATION\",\r\n                \"attributeName\": \"encounter location\",\r\n                \"assessment\": \"Encounter location is populated\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"Encounter location is not populated\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"encounterReason\": {},\r\n        \"encounterStatus\": {},\r\n        \"encounterType\": {\r\n          \"data\": {\r\n            \"text\": \"Psychiatric interview and evaluation (procedure)\"\r\n          },\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"100\",\r\n              \"attributeScoreWeighted\": \"100\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"1\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"ENC_TYPE\",\r\n                \"attributeName\": \"encounter type\",\r\n                \"assessment\": \"Encounter type is populated\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Passed\",\r\n                \"reason\": \"\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"elementAudit\": {\r\n          \"elementScore\": \"20\",\r\n          \"elementScoreWeighted\": \"20\",\r\n          \"elementCriticalFailureCount\": \"0\",\r\n          \"elementNumerator\": \"1\",\r\n          \"elementDenominator\": \"5\"\r\n        }\r\n      }\r\n    ],\r\n    \"goals\": [],\r\n    \"healthAssessments\": [\r\n      {\r\n        \"assessment\": {\r\n          \"data\": {\r\n            \"codings\": [\r\n              {\r\n                \"system\": \"http://loinc.org\",\r\n                \"code\": \"73831-0\",\r\n                \"display\": \"Adolescent depression screening assessment\"\r\n              }\r\n            ],\r\n            \"text\": \"Adolescent depression screening assessment\"\r\n          },\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"100\",\r\n              \"attributeScoreWeighted\": \"100\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"1\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"HA_ITEM\",\r\n                \"attributeName\": \"assessment\",\r\n                \"assessment\": \"Health status assessment is  LOINC or SNOMED-CT\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Passed\",\r\n                \"reason\": \"\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"effectiveDate\": {\r\n          \"data\": \"6/21/2026 4:00:00 AM\",\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"HA_EFFDT\",\r\n                \"attributeName\": \"effectiveDate\",\r\n                \"assessment\": \"Health Status assessment date is date in past\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"Health Status assessment date is not date in past\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"issueDateTime\": {},\r\n        \"resultUnit\": {},\r\n        \"resultValue\": {\r\n          \"data\": {\r\n            \"text\": \"Depression screening positive (finding)\",\r\n            \"type\": {\r\n              \"text\": \"CE\"\r\n            }\r\n          }\r\n        },\r\n        \"elementAudit\": {\r\n          \"elementScore\": \"50\",\r\n          \"elementScoreWeighted\": \"50\",\r\n          \"elementCriticalFailureCount\": \"0\",\r\n          \"elementNumerator\": \"1\",\r\n          \"elementDenominator\": \"2\"\r\n        }\r\n      }\r\n    ],\r\n    \"immunizations\": [],\r\n    \"labResults\": [],\r\n    \"medicalDevices\": [],\r\n    \"medications\": [],\r\n    \"procedures\": [\r\n      {\r\n        \"procedure\": {\r\n          \"data\": {\r\n            \"codings\": [\r\n              {\r\n                \"system\": \"http://snomed.info/sct\",\r\n                \"code\": \"108313002\",\r\n                \"display\": \"Family psychotherapy procedure (regime/therapy)\"\r\n              }\r\n            ],\r\n            \"text\": \"Family psychotherapy procedure (regime/therapy)\"\r\n          },\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"100\",\r\n              \"attributeScoreWeighted\": \"100\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"1\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"PRC_PROC\",\r\n                \"attributeName\": \"procedure\",\r\n                \"assessment\": \"Procedure is valid concept\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Passed\",\r\n                \"reason\": \"\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"procedurePerformedDate\": {},\r\n        \"procedureStatus\": {},\r\n        \"procedureDateTime\": {\r\n          \"data\": \"6/21/2026 4:00:00 AM\",\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"PRC_PRCDT\",\r\n                \"attributeName\": \"procedureDateTime\",\r\n                \"assessment\": \"Procedure date is valid\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"Procedure date is invalid\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"procedureReason\": {\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"PRC_RSN\",\r\n                \"attributeName\": \"procedureReason\",\r\n                \"assessment\": \"Procedure reason is populated\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"Procedure reason not populated\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"elementAudit\": {\r\n          \"elementScore\": \"33\",\r\n          \"elementScoreWeighted\": \"33\",\r\n          \"elementCriticalFailureCount\": \"0\",\r\n          \"elementNumerator\": \"1\",\r\n          \"elementDenominator\": \"3\"\r\n        }\r\n      }\r\n    ],\r\n    \"providers\": [],\r\n    \"vitalSigns\": []\r\n  }\r\n}", @"\s+", ""));
        #endregion

        #endregion
    }

    [Theory]
    [InlineData("/PIQI/ScoreMessage")]
    public async Task ScoresMessage4_ReturnsExpectedResponse(string endpoint)
    {
        // Arrange
        var piqiRequest = new PIQIRequest
        {
            DataProviderID = "TestProvider",
            DataSourceID = "TestSource",
            PIQIModelMnemonic = "PAT_CLINICAL_V1",
            EvaluationRubricMnemonic = "USCDI_V3",
            MessageID = "Msg004",
            MessageData = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData/Test4_PIQI.json"))
        };
        var result = new PIQIResponse();
        var requestContent = new StringContent(JsonConvert.SerializeObject(piqiRequest), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(endpoint, requestContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var contentType = response.Content.Headers.ContentType.MediaType;
        if (contentType == "text/plain" || contentType == "application/json")
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            result = JsonConvert.DeserializeObject<PIQIResponse>(responseBody);

            Assert.NotNull(result);
        }

        #region Check Results

        #region Overall Message Results
        Assert.Equal(result.Succeeded, true);
        Assert.Equal(result.ScoringData.MessageResults.PIQIScore, 45);
        Assert.Equal(result.ScoringData.MessageResults.Numerator, 5);
        Assert.Equal(result.ScoringData.MessageResults.Denominator, 11);
        Assert.Equal(result.ScoringData.MessageResults.CriticalFailureCount, 0);
        Assert.Equal(result.ScoringData.MessageResults.WeightedPIQIScore, 45);
        Assert.Equal(result.ScoringData.MessageResults.WeightedNumerator, 5);
        Assert.Equal(result.ScoringData.MessageResults.WeightedDenominator, 11);
        #endregion

        #region Data Class Results
        Assert.Equal(result.ScoringData.DataClassResults.Count, 15);

        #region Allergies
        var allergies = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Allergies");
        Assert.Equal(allergies.InstanceCount, 0);
        Assert.Equal(allergies.PIQIScore, 0);
        Assert.Equal(allergies.Numerator, 0);
        Assert.Equal(allergies.Denominator, 0);
        Assert.Equal(allergies.CriticalFailureCount, 0);
        Assert.Equal(allergies.WeightedPIQIScore, 0);
        Assert.Equal(allergies.WeightedNumerator, 0);
        Assert.Equal(allergies.WeightedDenominator, 0);
        #endregion

        #region Clinical Documents
        var clinicalDocuments = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Clinical Documents");
        Assert.Equal(clinicalDocuments.InstanceCount, 0);
        Assert.Equal(clinicalDocuments.PIQIScore, 0);
        Assert.Equal(clinicalDocuments.Numerator, 0);
        Assert.Equal(clinicalDocuments.Denominator, 0);
        Assert.Equal(clinicalDocuments.CriticalFailureCount, 0);
        Assert.Equal(clinicalDocuments.WeightedPIQIScore, 0);
        Assert.Equal(clinicalDocuments.WeightedNumerator, 0);
        Assert.Equal(clinicalDocuments.WeightedDenominator, 0);
        #endregion

        #region Conditions
        var conditions = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Conditions");
        Assert.Equal(conditions.InstanceCount, 0);
        Assert.Equal(conditions.PIQIScore, 0);
        Assert.Equal(conditions.Numerator, 0);
        Assert.Equal(conditions.Denominator, 0);
        Assert.Equal(conditions.CriticalFailureCount, 0);
        Assert.Equal(conditions.WeightedPIQIScore, 0);
        Assert.Equal(conditions.WeightedNumerator, 0);
        Assert.Equal(conditions.WeightedDenominator, 0);
        #endregion

        #region Demographics
        var demographics = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Demographics");
        Assert.Equal(demographics.InstanceCount, 1);
        Assert.Equal(demographics.PIQIScore, 75);
        Assert.Equal(demographics.Numerator, 3);
        Assert.Equal(demographics.Denominator, 4);
        Assert.Equal(demographics.CriticalFailureCount, 0);
        Assert.Equal(demographics.WeightedPIQIScore, 75);
        Assert.Equal(demographics.WeightedNumerator, 3);
        Assert.Equal(demographics.WeightedDenominator, 4);
        #endregion

        #region Diagnostic Imaging
        var diagnosticImaging = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Diagnostic Imaging");
        Assert.Equal(diagnosticImaging.InstanceCount, 0);
        Assert.Equal(diagnosticImaging.PIQIScore, 0);
        Assert.Equal(diagnosticImaging.Numerator, 0);
        Assert.Equal(diagnosticImaging.Denominator, 0);
        Assert.Equal(diagnosticImaging.CriticalFailureCount, 0);
        Assert.Equal(diagnosticImaging.WeightedPIQIScore, 0);
        Assert.Equal(diagnosticImaging.WeightedNumerator, 0);
        Assert.Equal(diagnosticImaging.WeightedDenominator, 0);
        #endregion

        #region Encounters
        var encounters = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Encounters");
        Assert.Equal(encounters.InstanceCount, 1);
        Assert.Equal(encounters.PIQIScore, 20);
        Assert.Equal(encounters.Numerator, 1);
        Assert.Equal(encounters.Denominator, 5);
        Assert.Equal(encounters.CriticalFailureCount, 0);
        Assert.Equal(encounters.WeightedPIQIScore, 20);
        Assert.Equal(encounters.WeightedNumerator, 1);
        Assert.Equal(encounters.WeightedDenominator, 5);
        #endregion

        #region Goals
        var goals = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Goals");
        Assert.Equal(goals.InstanceCount, 0);
        Assert.Equal(goals.PIQIScore, 0);
        Assert.Equal(goals.Numerator, 0);
        Assert.Equal(goals.Denominator, 0);
        Assert.Equal(goals.CriticalFailureCount, 0);
        Assert.Equal(goals.WeightedPIQIScore, 0);
        Assert.Equal(goals.WeightedNumerator, 0);
        Assert.Equal(goals.WeightedDenominator, 0);
        #endregion

        #region Health Assessments
        var healthAssessments = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Health Assessments");
        Assert.Equal(healthAssessments.InstanceCount, 1);
        Assert.Equal(healthAssessments.PIQIScore, 50);
        Assert.Equal(healthAssessments.Numerator, 1);
        Assert.Equal(healthAssessments.Denominator, 2);
        Assert.Equal(healthAssessments.CriticalFailureCount, 0);
        Assert.Equal(healthAssessments.WeightedPIQIScore, 50);
        Assert.Equal(healthAssessments.WeightedNumerator, 1);
        Assert.Equal(healthAssessments.WeightedDenominator, 2);
        #endregion

        #region Immunizations
        var immunizations = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Immunizations");
        Assert.Equal(immunizations.InstanceCount, 0);
        Assert.Equal(immunizations.PIQIScore, 0);
        Assert.Equal(immunizations.Numerator, 0);
        Assert.Equal(immunizations.Denominator, 0);
        Assert.Equal(immunizations.CriticalFailureCount, 0);
        Assert.Equal(immunizations.WeightedPIQIScore, 0);
        Assert.Equal(immunizations.WeightedNumerator, 0);
        Assert.Equal(immunizations.WeightedDenominator, 0);
        #endregion

        #region Lab Results
        var labResults = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Lab Results");
        Assert.Equal(labResults.InstanceCount, 0);
        Assert.Equal(labResults.PIQIScore, 0);
        Assert.Equal(labResults.Numerator, 0);
        Assert.Equal(labResults.Denominator, 0);
        Assert.Equal(labResults.CriticalFailureCount, 0);
        Assert.Equal(labResults.WeightedPIQIScore, 0);
        Assert.Equal(labResults.WeightedNumerator, 0);
        Assert.Equal(labResults.WeightedDenominator, 0);
        #endregion

        #region Medical Devices
        var medicalDevices = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Medical Devices");
        Assert.Equal(medicalDevices.InstanceCount, 0);
        Assert.Equal(medicalDevices.PIQIScore, 0);
        Assert.Equal(medicalDevices.Numerator, 0);
        Assert.Equal(medicalDevices.Denominator, 0);
        Assert.Equal(medicalDevices.CriticalFailureCount, 0);
        Assert.Equal(medicalDevices.WeightedPIQIScore, 0);
        Assert.Equal(medicalDevices.WeightedNumerator, 0);
        Assert.Equal(medicalDevices.WeightedDenominator, 0);
        #endregion

        #region Medications
        var medications = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Medications");
        Assert.Equal(medications.InstanceCount, 0);
        Assert.Equal(medications.PIQIScore, 0);
        Assert.Equal(medications.Numerator, 0);
        Assert.Equal(medications.Denominator, 0);
        Assert.Equal(medications.CriticalFailureCount, 0);
        Assert.Equal(medications.WeightedPIQIScore, 0);
        Assert.Equal(medications.WeightedNumerator, 0);
        Assert.Equal(medications.WeightedDenominator, 0);
        #endregion

        #region Procedures
        var procedures = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Procedures");
        Assert.Equal(procedures.InstanceCount, 0);
        Assert.Equal(procedures.PIQIScore, 0);
        Assert.Equal(procedures.Numerator, 0);
        Assert.Equal(procedures.Denominator, 0);
        Assert.Equal(procedures.CriticalFailureCount, 0);
        Assert.Equal(procedures.WeightedPIQIScore, 0);
        Assert.Equal(procedures.WeightedNumerator, 0);
        Assert.Equal(procedures.WeightedDenominator, 0);
        #endregion

        #region Vital Signs
        var vitalSigns = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Vital Signs");
        Assert.Equal(vitalSigns.InstanceCount, 0);
        Assert.Equal(vitalSigns.PIQIScore, 0);
        Assert.Equal(vitalSigns.Numerator, 0);
        Assert.Equal(vitalSigns.Denominator, 0);
        Assert.Equal(vitalSigns.CriticalFailureCount, 0);
        Assert.Equal(vitalSigns.WeightedPIQIScore, 0);
        Assert.Equal(vitalSigns.WeightedNumerator, 0);
        Assert.Equal(vitalSigns.WeightedDenominator, 0);
        #endregion

        #endregion

        #region Informational Results
        Assert.Equal(result.ScoringData.InformationalResults.Count, 15);

        #region Allergies
        var allergiesInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Allergies");
        Assert.Equal(allergiesInfo.EvaluationList.Count, 0);
        #endregion

        #region Clinical Documents
        var clinicalDocumentsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Clinical Documents");
        Assert.Equal(clinicalDocumentsInfo.EvaluationList.Count, 0);
        #endregion

        #region Conditions
        var conditionsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Conditions");
        Assert.Equal(conditionsInfo.EvaluationList.Count, 0);
        #endregion

        #region Demographics
        var demographicsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Demographics");
        Assert.Equal(demographicsInfo.EvaluationList.Count, 0);
        #endregion

        #region Diagnostic Imaging
        var diagnosticImagingInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Diagnostic Imaging");
        Assert.Equal(diagnosticImagingInfo.EvaluationList.Count, 0);
        #endregion

        #region Encounters
        var encountersInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Encounters");
        Assert.Equal(encountersInfo.EvaluationList.Count, 0);
        #endregion

        #region Goals
        var goalsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Goals");
        Assert.Equal(goalsInfo.EvaluationList.Count, 0);
        #endregion

        #region Health Assessments
        var healthAssessmentsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Health Assessments");
        Assert.Equal(healthAssessmentsInfo.EvaluationList.Count, 0);
        #endregion

        #region Immunizations
        var immunizationsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Immunizations");
        Assert.Equal(immunizationsInfo.EvaluationList.Count, 0);
        #endregion

        #region Lab Results
        var labResultsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Lab Results");
        Assert.Equal(labResultsInfo.EvaluationList.Count, 0);
        #endregion

        #region Medical Devices
        var medicalDevicesInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Medical Devices");
        Assert.Equal(medicalDevicesInfo.EvaluationList.Count, 0);
        #endregion

        #region Medications
        var medicationsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Medications");
        Assert.Equal(medicationsInfo.EvaluationList.Count, 0);
        #endregion

        #region Procedures
        var proceduresInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Procedures");
        Assert.Equal(proceduresInfo.EvaluationList.Count, 0);
        #endregion

        #region Vital Signs
        var vitalSignsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Vital Signs");
        Assert.Equal(vitalSignsInfo.EvaluationList.Count, 0);
        #endregion

        #endregion

        #endregion
    }

    [Theory]
    [InlineData("/PIQI/ScoreAuditMessage")]
    public async Task ScoreAuditMessage4_ReturnsExpectedResponse(string endpoint)
    {
        // Arrange
        var piqiRequest = new PIQIRequest
        {
            DataProviderID = "TestProvider",
            DataSourceID = "TestSource",
            PIQIModelMnemonic = "PAT_CLINICAL_V1",
            EvaluationRubricMnemonic = "USCDI_V3",
            MessageID = "Msg004",
            MessageData = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData/Test4_PIQI.json"))
        };
        var result = new PIQIResponse();
        var requestContent = new StringContent(JsonConvert.SerializeObject(piqiRequest), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(endpoint, requestContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var contentType = response.Content.Headers.ContentType.MediaType;
        if (contentType == "text/plain" || contentType == "application/json")
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            result = JsonConvert.DeserializeObject<PIQIResponse>(responseBody);

            Assert.NotNull(result);
        }

        #region Check Results

        #region Overall Message Results
        Assert.Equal(result.Succeeded, true);
        Assert.Equal(result.ScoringData.MessageResults.PIQIScore, 45);
        Assert.Equal(result.ScoringData.MessageResults.Numerator, 5);
        Assert.Equal(result.ScoringData.MessageResults.Denominator, 11);
        Assert.Equal(result.ScoringData.MessageResults.CriticalFailureCount, 0);
        Assert.Equal(result.ScoringData.MessageResults.WeightedPIQIScore, 45);
        Assert.Equal(result.ScoringData.MessageResults.WeightedNumerator, 5);
        Assert.Equal(result.ScoringData.MessageResults.WeightedDenominator, 11);
        #endregion

        #region Data Class Results
        Assert.Equal(result.ScoringData.DataClassResults.Count, 15);

        #region Allergies
        var allergies = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Allergies");
        Assert.Equal(allergies.InstanceCount, 0);
        Assert.Equal(allergies.PIQIScore, 0);
        Assert.Equal(allergies.Numerator, 0);
        Assert.Equal(allergies.Denominator, 0);
        Assert.Equal(allergies.CriticalFailureCount, 0);
        Assert.Equal(allergies.WeightedPIQIScore, 0);
        Assert.Equal(allergies.WeightedNumerator, 0);
        Assert.Equal(allergies.WeightedDenominator, 0);
        #endregion

        #region Clinical Documents
        var clinicalDocuments = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Clinical Documents");
        Assert.Equal(clinicalDocuments.InstanceCount, 0);
        Assert.Equal(clinicalDocuments.PIQIScore, 0);
        Assert.Equal(clinicalDocuments.Numerator, 0);
        Assert.Equal(clinicalDocuments.Denominator, 0);
        Assert.Equal(clinicalDocuments.CriticalFailureCount, 0);
        Assert.Equal(clinicalDocuments.WeightedPIQIScore, 0);
        Assert.Equal(clinicalDocuments.WeightedNumerator, 0);
        Assert.Equal(clinicalDocuments.WeightedDenominator, 0);
        #endregion

        #region Conditions
        var conditions = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Conditions");
        Assert.Equal(conditions.InstanceCount, 0);
        Assert.Equal(conditions.PIQIScore, 0);
        Assert.Equal(conditions.Numerator, 0);
        Assert.Equal(conditions.Denominator, 0);
        Assert.Equal(conditions.CriticalFailureCount, 0);
        Assert.Equal(conditions.WeightedPIQIScore, 0);
        Assert.Equal(conditions.WeightedNumerator, 0);
        Assert.Equal(conditions.WeightedDenominator, 0);
        #endregion

        #region Demographics
        var demographics = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Demographics");
        Assert.Equal(demographics.InstanceCount, 1);
        Assert.Equal(demographics.PIQIScore, 75);
        Assert.Equal(demographics.Numerator, 3);
        Assert.Equal(demographics.Denominator, 4);
        Assert.Equal(demographics.CriticalFailureCount, 0);
        Assert.Equal(demographics.WeightedPIQIScore, 75);
        Assert.Equal(demographics.WeightedNumerator, 3);
        Assert.Equal(demographics.WeightedDenominator, 4);
        #endregion

        #region Diagnostic Imaging
        var diagnosticImaging = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Diagnostic Imaging");
        Assert.Equal(diagnosticImaging.InstanceCount, 0);
        Assert.Equal(diagnosticImaging.PIQIScore, 0);
        Assert.Equal(diagnosticImaging.Numerator, 0);
        Assert.Equal(diagnosticImaging.Denominator, 0);
        Assert.Equal(diagnosticImaging.CriticalFailureCount, 0);
        Assert.Equal(diagnosticImaging.WeightedPIQIScore, 0);
        Assert.Equal(diagnosticImaging.WeightedNumerator, 0);
        Assert.Equal(diagnosticImaging.WeightedDenominator, 0);
        #endregion

        #region Encounters
        var encounters = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Encounters");
        Assert.Equal(encounters.InstanceCount, 1);
        Assert.Equal(encounters.PIQIScore, 20);
        Assert.Equal(encounters.Numerator, 1);
        Assert.Equal(encounters.Denominator, 5);
        Assert.Equal(encounters.CriticalFailureCount, 0);
        Assert.Equal(encounters.WeightedPIQIScore, 20);
        Assert.Equal(encounters.WeightedNumerator, 1);
        Assert.Equal(encounters.WeightedDenominator, 5);
        #endregion

        #region Goals
        var goals = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Goals");
        Assert.Equal(goals.InstanceCount, 0);
        Assert.Equal(goals.PIQIScore, 0);
        Assert.Equal(goals.Numerator, 0);
        Assert.Equal(goals.Denominator, 0);
        Assert.Equal(goals.CriticalFailureCount, 0);
        Assert.Equal(goals.WeightedPIQIScore, 0);
        Assert.Equal(goals.WeightedNumerator, 0);
        Assert.Equal(goals.WeightedDenominator, 0);
        #endregion

        #region Health Assessments
        var healthAssessments = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Health Assessments");
        Assert.Equal(healthAssessments.InstanceCount, 1);
        Assert.Equal(healthAssessments.PIQIScore, 50);
        Assert.Equal(healthAssessments.Numerator, 1);
        Assert.Equal(healthAssessments.Denominator, 2);
        Assert.Equal(healthAssessments.CriticalFailureCount, 0);
        Assert.Equal(healthAssessments.WeightedPIQIScore, 50);
        Assert.Equal(healthAssessments.WeightedNumerator, 1);
        Assert.Equal(healthAssessments.WeightedDenominator, 2);
        #endregion

        #region Immunizations
        var immunizations = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Immunizations");
        Assert.Equal(immunizations.InstanceCount, 0);
        Assert.Equal(immunizations.PIQIScore, 0);
        Assert.Equal(immunizations.Numerator, 0);
        Assert.Equal(immunizations.Denominator, 0);
        Assert.Equal(immunizations.CriticalFailureCount, 0);
        Assert.Equal(immunizations.WeightedPIQIScore, 0);
        Assert.Equal(immunizations.WeightedNumerator, 0);
        Assert.Equal(immunizations.WeightedDenominator, 0);
        #endregion

        #region Lab Results
        var labResults = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Lab Results");
        Assert.Equal(labResults.InstanceCount, 0);
        Assert.Equal(labResults.PIQIScore, 0);
        Assert.Equal(labResults.Numerator, 0);
        Assert.Equal(labResults.Denominator, 0);
        Assert.Equal(labResults.CriticalFailureCount, 0);
        Assert.Equal(labResults.WeightedPIQIScore, 0);
        Assert.Equal(labResults.WeightedNumerator, 0);
        Assert.Equal(labResults.WeightedDenominator, 0);
        #endregion

        #region Medical Devices
        var medicalDevices = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Medical Devices");
        Assert.Equal(medicalDevices.InstanceCount, 0);
        Assert.Equal(medicalDevices.PIQIScore, 0);
        Assert.Equal(medicalDevices.Numerator, 0);
        Assert.Equal(medicalDevices.Denominator, 0);
        Assert.Equal(medicalDevices.CriticalFailureCount, 0);
        Assert.Equal(medicalDevices.WeightedPIQIScore, 0);
        Assert.Equal(medicalDevices.WeightedNumerator, 0);
        Assert.Equal(medicalDevices.WeightedDenominator, 0);
        #endregion

        #region Medications
        var medications = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Medications");
        Assert.Equal(medications.InstanceCount, 0);
        Assert.Equal(medications.PIQIScore, 0);
        Assert.Equal(medications.Numerator, 0);
        Assert.Equal(medications.Denominator, 0);
        Assert.Equal(medications.CriticalFailureCount, 0);
        Assert.Equal(medications.WeightedPIQIScore, 0);
        Assert.Equal(medications.WeightedNumerator, 0);
        Assert.Equal(medications.WeightedDenominator, 0);
        #endregion

        #region Procedures
        var procedures = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Procedures");
        Assert.Equal(procedures.InstanceCount, 0);
        Assert.Equal(procedures.PIQIScore, 0);
        Assert.Equal(procedures.Numerator, 0);
        Assert.Equal(procedures.Denominator, 0);
        Assert.Equal(procedures.CriticalFailureCount, 0);
        Assert.Equal(procedures.WeightedPIQIScore, 0);
        Assert.Equal(procedures.WeightedNumerator, 0);
        Assert.Equal(procedures.WeightedDenominator, 0);
        #endregion

        #region Vital Signs
        var vitalSigns = result.ScoringData.DataClassResults.FirstOrDefault(dcr => dcr.DataClassName == "Vital Signs");
        Assert.Equal(vitalSigns.InstanceCount, 0);
        Assert.Equal(vitalSigns.PIQIScore, 0);
        Assert.Equal(vitalSigns.Numerator, 0);
        Assert.Equal(vitalSigns.Denominator, 0);
        Assert.Equal(vitalSigns.CriticalFailureCount, 0);
        Assert.Equal(vitalSigns.WeightedPIQIScore, 0);
        Assert.Equal(vitalSigns.WeightedNumerator, 0);
        Assert.Equal(vitalSigns.WeightedDenominator, 0);
        #endregion

        #endregion

        #region Informational Results
        Assert.Equal(result.ScoringData.InformationalResults.Count, 15);

        #region Allergies
        var allergiesInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Allergies");
        Assert.Equal(allergiesInfo.EvaluationList.Count, 0);
        #endregion

        #region Clinical Documents
        var clinicalDocumentsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Clinical Documents");
        Assert.Equal(clinicalDocumentsInfo.EvaluationList.Count, 0);
        #endregion

        #region Conditions
        var conditionsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Conditions");
        Assert.Equal(conditionsInfo.EvaluationList.Count, 0);
        #endregion

        #region Demographics
        var demographicsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Demographics");
        Assert.Equal(demographicsInfo.EvaluationList.Count, 0);
        #endregion

        #region Diagnostic Imaging
        var diagnosticImagingInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Diagnostic Imaging");
        Assert.Equal(diagnosticImagingInfo.EvaluationList.Count, 0);
        #endregion

        #region Encounters
        var encountersInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Encounters");
        Assert.Equal(encountersInfo.EvaluationList.Count, 0);
        #endregion

        #region Goals
        var goalsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Goals");
        Assert.Equal(goalsInfo.EvaluationList.Count, 0);
        #endregion

        #region Health Assessments
        var healthAssessmentsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Health Assessments");
        Assert.Equal(healthAssessmentsInfo.EvaluationList.Count, 0);
        #endregion

        #region Immunizations
        var immunizationsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Immunizations");
        Assert.Equal(immunizationsInfo.EvaluationList.Count, 0);
        #endregion

        #region Lab Results
        var labResultsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Lab Results");
        Assert.Equal(labResultsInfo.EvaluationList.Count, 0);
        #endregion

        #region Medical Devices
        var medicalDevicesInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Medical Devices");
        Assert.Equal(medicalDevicesInfo.EvaluationList.Count, 0);
        #endregion

        #region Medications
        var medicationsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Medications");
        Assert.Equal(medicationsInfo.EvaluationList.Count, 0);
        #endregion

        #region Procedures
        var proceduresInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Procedures");
        Assert.Equal(proceduresInfo.EvaluationList.Count, 0);
        #endregion

        #region Vital Signs
        var vitalSignsInfo = result.ScoringData.InformationalResults.FirstOrDefault(ir => ir.DataClassName == "Vital Signs");
        Assert.Equal(vitalSignsInfo.EvaluationList.Count, 0);
        #endregion

        #endregion

        #region Audit Results
        Assert.Equal(Regex.Replace(result.AuditedMessage, @"\s+", ""), Regex.Replace("{\r\n  \"EntityModelMnemonic\": \"PAT_CLINICAL_V1\",\r\n  \"DataProviderID\": \"TestProvider\",\r\n  \"DataSourceID\": \"TestSource\",\r\n  \"MessageID\": \"Msg004\",\r\n  \"Audit\": {\r\n    \"messageNumerator\": \"5\",\r\n    \"messageDenominator\": \"11\",\r\n    \"messageScore\": \"45\",\r\n    \"messageNumeratorWeighted\": \"5\",\r\n    \"messageDenominatorWeighted\": \"11\",\r\n    \"messageScoreWeighted\": \"45\",\r\n    \"messageCriticalFailureCount\": \"0\"\r\n  },\r\n  \"patient\": {\r\n    \"allergies\": [],\r\n    \"clinicalDocuments\": [],\r\n    \"conditions\": [],\r\n    \"demographics\": [\r\n      {\r\n        \"birthDate\": {\r\n          \"data\": \"2009-01-01\",\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"100\",\r\n              \"attributeScoreWeighted\": \"100\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"1\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"DEM_DOB\",\r\n                \"attributeName\": \"birthDate\",\r\n                \"assessment\": \"Date of birth is valid past date\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Passed\",\r\n                \"reason\": \"\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"birthSex\": {\r\n          \"data\": {\r\n            \"codings\": [\r\n              {\r\n                \"system\": \"http://hl7.org/fhir/administrative-gender\",\r\n                \"code\": \"male\",\r\n                \"display\": \"male\"\r\n              }\r\n            ],\r\n            \"text\": \"male\"\r\n          },\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"DEM_SEX\",\r\n                \"attributeName\": \"birthSex\",\r\n                \"assessment\": \"Birth sex is SNOMED-CT\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"invalid concept\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"deathDate\": {},\r\n        \"deceased\": {},\r\n        \"ethnicity\": {\r\n          \"data\": {\r\n            \"codings\": [\r\n              {\r\n                \"system\": \"urn:oid:2.16.840.1.113883.6.238\",\r\n                \"code\": \"2135-2\",\r\n                \"display\": \"Hispanic or Latino\"\r\n              }\r\n            ],\r\n            \"text\": \"Hispanic or Latino\"\r\n          },\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"100\",\r\n              \"attributeScoreWeighted\": \"100\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"1\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"DEM_ETHN\",\r\n                \"attributeName\": \"ethnicity\",\r\n                \"assessment\": \"Ethnicity is valid code\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Passed\",\r\n                \"reason\": \"\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"genderIdentity\": {},\r\n        \"maritalStatus\": {},\r\n        \"patientIdentifier\": {},\r\n        \"primaryLanguage\": {},\r\n        \"race\": {\r\n          \"data\": {\r\n            \"codings\": [\r\n              {\r\n                \"system\": \"urn:oid:2.16.840.1.113883.6.238\",\r\n                \"code\": \"1002-5\",\r\n                \"display\": \"American Indian or Alaska Native\"\r\n              }\r\n            ],\r\n            \"text\": \"American Indian or Alaska Native\"\r\n          },\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"100\",\r\n              \"attributeScoreWeighted\": \"100\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"1\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"DEM_RACE\",\r\n                \"attributeName\": \"race\",\r\n                \"assessment\": \"Race is valid concept\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Passed\",\r\n                \"reason\": \"\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"elementAudit\": {\r\n          \"elementScore\": \"75\",\r\n          \"elementScoreWeighted\": \"75\",\r\n          \"elementCriticalFailureCount\": \"0\",\r\n          \"elementNumerator\": \"3\",\r\n          \"elementDenominator\": \"4\"\r\n        }\r\n      }\r\n    ],\r\n    \"diagnosticImaging\": [],\r\n    \"encounters\": [\r\n      {\r\n        \"encounterDateTime\": {\r\n          \"data\": \"6/21/2026 4:00:00 AM\",\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"ENC_DATETIME\",\r\n                \"attributeName\": \"encounter date/time\",\r\n                \"assessment\": \"Encounter date is in the past\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"Encounter date is in not the past\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"encounterDiagnosis\": {\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"ENC_DIAGNOSIS\",\r\n                \"attributeName\": \"encounter diagnosis\",\r\n                \"assessment\": \"Diagnosis is SNOMED-CT or ICD-10-CM\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"unpopulated\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"encounterDisposition\": {\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"ENC_DISPOSITION\",\r\n                \"attributeName\": \"encounter disposition\",\r\n                \"assessment\": \"Encounter disposition is populated\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"Encounter disposition is not populated\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"encounterEndDateTime\": {},\r\n        \"encounterIdentifier\": {},\r\n        \"encounterLocation\": {\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"ENC_LOCATION\",\r\n                \"attributeName\": \"encounter location\",\r\n                \"assessment\": \"Encounter location is populated\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"Encounter location is not populated\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"encounterReason\": {},\r\n        \"encounterStatus\": {},\r\n        \"encounterType\": {\r\n          \"data\": {\r\n            \"text\": \"Psychiatric interview and evaluation (procedure)\"\r\n          },\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"100\",\r\n              \"attributeScoreWeighted\": \"100\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"1\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"ENC_TYPE\",\r\n                \"attributeName\": \"encounter type\",\r\n                \"assessment\": \"Encounter type is populated\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Passed\",\r\n                \"reason\": \"\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"elementAudit\": {\r\n          \"elementScore\": \"20\",\r\n          \"elementScoreWeighted\": \"20\",\r\n          \"elementCriticalFailureCount\": \"0\",\r\n          \"elementNumerator\": \"1\",\r\n          \"elementDenominator\": \"5\"\r\n        }\r\n      }\r\n    ],\r\n    \"goals\": [],\r\n    \"healthAssessments\": [\r\n      {\r\n        \"assessment\": {\r\n          \"data\": {\r\n            \"codings\": [\r\n              {\r\n                \"system\": \"http://loinc.org\",\r\n                \"code\": \"73831-0\",\r\n                \"display\": \"Adolescent depression screening assessment\"\r\n              }\r\n            ],\r\n            \"text\": \"Adolescent depression screening assessment\"\r\n          },\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"100\",\r\n              \"attributeScoreWeighted\": \"100\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"1\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"HA_ITEM\",\r\n                \"attributeName\": \"assessment\",\r\n                \"assessment\": \"Health status assessment is  LOINC or SNOMED-CT\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Passed\",\r\n                \"reason\": \"\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"effectiveDate\": {\r\n          \"data\": \"6/15/2026 4:00:00 AM\",\r\n          \"attributeAudit\": {\r\n            \"scoringData\": {\r\n              \"attributeScore\": \"0\",\r\n              \"attributeScoreWeighted\": \"0\",\r\n              \"attributeCriticalFailureCount\": \"0\",\r\n              \"attributeNumerator\": \"0\",\r\n              \"attributeDenominator\": \"1\"\r\n            },\r\n            \"assessmentItems\": [\r\n              {\r\n                \"attributeMnemonic\": \"HA_EFFDT\",\r\n                \"attributeName\": \"effectiveDate\",\r\n                \"assessment\": \"Health Status assessment date is date in past\",\r\n                \"effect\": \"Scoring\",\r\n                \"status\": \"Failed\",\r\n                \"reason\": \"Health Status assessment date is not date in past\"\r\n              }\r\n            ],\r\n            \"InformationalItems\": []\r\n          }\r\n        },\r\n        \"issueDateTime\": {},\r\n        \"resultUnit\": {},\r\n        \"resultValue\": {\r\n          \"data\": {\r\n            \"text\": \"Depression screening negative (finding)\",\r\n            \"type\": {\r\n              \"text\": \"CE\"\r\n            }\r\n          }\r\n        },\r\n        \"elementAudit\": {\r\n          \"elementScore\": \"50\",\r\n          \"elementScoreWeighted\": \"50\",\r\n          \"elementCriticalFailureCount\": \"0\",\r\n          \"elementNumerator\": \"1\",\r\n          \"elementDenominator\": \"2\"\r\n        }\r\n      }\r\n    ],\r\n    \"immunizations\": [],\r\n    \"labResults\": [],\r\n    \"medicalDevices\": [],\r\n    \"medications\": [],\r\n    \"procedures\": [],\r\n    \"providers\": [],\r\n    \"vitalSigns\": []\r\n  }\r\n}", @"\s+", ""));
        #endregion

        #endregion
    }
}