/**
 * Attribute formatting utilities
 * Responsible for extracting readable values from complex nested objects
 */

function isPrimitiveValue(value) {
  return value === null || value === undefined || ['string', 'number', 'boolean'].includes(typeof value);
}

function simplifyAttributeValue(attributeName, dataClass, rawValue) {
  if (!rawValue) return 'N/A';
  const str = String(rawValue);
  if (str.length > 100) {
    return str.slice(0, 97) + '...';
  }
  return str;
}

/**
 * Format an attribute value for database storage
 * Intelligently extracts readable values from nested objects, arrays, or codings
 * @param {string} attributeName - Name of the attribute (used to find format preferences)
 * @param {*} value - The value to format
 * @param {Object} config - Configuration object with attributeFormatPreferences and genericPreferredFields
 * @returns {string} Formatted, readable value
 */
function formatObjectAttributeValue(attributeName, value, config) {
  if (value === null || value === undefined) {
    return 'N/A';
  }

  if (Array.isArray(value)) {
    if (value.length === 0) {
      return 'N/A';
    }

    const primitiveItems = value.filter(isPrimitiveValue).map((item) => String(item).trim()).filter(Boolean);
    if (primitiveItems.length > 0) {
      return primitiveItems.join(', ');
    }

    return JSON.stringify(value);
  }

  if (isPrimitiveValue(value)) {
    return String(value).trim() || 'N/A';
  }

  const preferredKeys = config.attributeFormatPreferences[attributeName] || config.genericPreferredFields;

  for (const key of preferredKeys) {
    if (Object.prototype.hasOwnProperty.call(value, key) && isPrimitiveValue(value[key])) {
      const formatted = String(value[key]).trim();
      if (formatted) {
        return formatted;
      }
    }
  }

  if (Array.isArray(value.codings)) {
    const firstCoding = value.codings.find((coding) => coding && (coding.code || coding.display || coding.text));
    if (firstCoding) {
      const codingValue = String(firstCoding.display || firstCoding.text || firstCoding.code || '').trim();
      if (codingValue) {
        return codingValue;
      }
    }
  }

  const primitiveEntries = Object.entries(value)
    .filter(([, entryValue]) => isPrimitiveValue(entryValue))
    .map(([entryKey, entryValue]) => `${entryKey}: ${entryValue}`)
    .filter(Boolean);

  if (primitiveEntries.length === 1) {
    return primitiveEntries[0].split(': ').slice(1).join(': ') || primitiveEntries[0];
  }

  if (primitiveEntries.length > 1) {
    return primitiveEntries.join(' | ');
  }

  return JSON.stringify(value);
}

module.exports = {
  isPrimitiveValue,
  simplifyAttributeValue,
  formatObjectAttributeValue,
};
