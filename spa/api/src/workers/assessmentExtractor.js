/**
 * Assessment extraction from scorer responses
 * Responsible for walking the audited message and extracting assessment items
 */

const { simplifyAttributeValue, formatObjectAttributeValue } = require('./attributeFormatter');

function getAssessmentItemValue(item) {
  if (!item || typeof item !== 'object') {
    return '';
  }

  const candidateKeys = [
    'attributeValue',
    'value',
    'displayValue',
    'text',
    'display',
    'name',
    'code',
  ];

  for (const key of candidateKeys) {
    const candidate = item[key];
    if (candidate === null || candidate === undefined) {
      continue;
    }
    const formatted = String(candidate).trim();
    if (formatted) {
      return formatted;
    }
  }

  return '';
}

function normalizeStatus(status, config) {
  if (!status) return 'Unknown';
  const lower = String(status).toLowerCase();
  if (config.statusNormalization[lower]) {
    return config.statusNormalization[lower];
  }
  return String(status);
}

function summarizeAssessmentStatuses(items) {
  const summary = { pass: 0, fail: 0, skip: 0 };
  (items || []).forEach((item) => {
    const status = (item.status || '').toLowerCase();
    if (status === 'pass') summary.pass += 1;
    else if (status === 'fail') summary.fail += 1;
    else summary.skip += 1;
  });
  return summary;
}

/**
 * Extract all assessment items from scorer response
 * @param {Object} response - Response from scorer
 * @param {string} messageId - Message ID
 * @param {number} rowNum - Row number for logging
 * @param {Object} config - Configuration with dataClassMap and attributeFormatPreferences
 * @returns {Array} Assessment items with metadata
 */
function extractAssessmentItemsFromResponse(response, messageId, rowNum, config) {
  const items = [];
  let auditedMessage = response.auditedMessage;
  
  if (typeof auditedMessage === 'string') {
    try {
      auditedMessage = JSON.parse(auditedMessage);
    } catch (_) {
      console.warn(`Row ${rowNum}: Could not parse auditedMessage string`);
      return items;
    }
  }
  
  if (!auditedMessage) {
    console.warn(`Row ${rowNum}: No auditedMessage`);
    return items;
  }
  
  const patientData = auditedMessage.patient || auditedMessage;
  
  Object.entries(config.dataClassMap).forEach(([className, fieldPath]) => {
    const classData = patientData[fieldPath];
    if (!Array.isArray(classData)) return;
    
    classData.forEach((element) => {
      const elementKeys = Object.keys(element || {});
      elementKeys.forEach((attrKey) => {
        const attribute = element[attrKey];
        if (!attribute || !attribute.attributeAudit || !Array.isArray(attribute.attributeAudit.assessmentItems)) {
          return;
        }
        
        attribute.attributeAudit.assessmentItems.forEach((item) => {
          let rawValue = 'N/A';
          let displayValue = 'N/A';
          
          if (attribute.data !== undefined && attribute.data !== null) {
            if (typeof attribute.data === 'object') {
              rawValue = JSON.stringify(attribute.data);
              displayValue = simplifyAttributeValue(
                item.attributeName || attrKey,
                className,
                formatObjectAttributeValue(item.attributeName || attrKey, attribute.data, config)
              );
            } else {
              rawValue = String(attribute.data);
              displayValue = simplifyAttributeValue(item.attributeName || attrKey, className, rawValue);
            }
          }

          if (displayValue === 'N/A') {
            const fallbackItemValue = getAssessmentItemValue(item);
            if (fallbackItemValue) {
              rawValue = fallbackItemValue;
              displayValue = simplifyAttributeValue(item.attributeName || attrKey, className, fallbackItemValue);
            }
          }
          
          const status = normalizeStatus(item.status, config);
          
          const assessmentItem = {
            messageId,
            rowNum,
            dataClass: className,
            attributeName: item.attributeName || attrKey,
            attributeValue: displayValue,
            rawAttributeValue: rawValue,
            assessment: item.assessment || item.samDisplayName || '-',
            status,
            reason: item.reason || item.errorMessage || item.customErrorMessage || '-',
            effect: item.effect || (item.isCritical ? 'Critical' : item.isScoring ? 'Scoring' : 'Conditional'),
          };
          
          items.push(assessmentItem);
        });
      });
    });
  });
  
  return items;
}

module.exports = {
  normalizeStatus,
  summarizeAssessmentStatuses,
  extractAssessmentItemsFromResponse,
};
