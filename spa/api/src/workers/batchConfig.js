/**
 * Batch processing configuration
 * Centralized config for worker behavior, mappings, and system codes
 */

const DEFAULT_BATCH_CONFIG = {
  dataClassMap: {
    'Lab Results': 'labResults',
    'Medications': 'medications',
    'Allergies': 'allergies',
    'Conditions': 'conditions',
    'Procedures': 'procedures',
    'Vital Signs': 'vitalSigns',
    'Immunizations': 'immunizations',
    'Demographics': 'demographics',
    'Encounters': 'encounters',
    'Providers': 'providers',
    'Clinical Documents': 'clinicalDocuments',
    'Diagnostic Imaging': 'diagnosticImaging',
    'Goals': 'goals',
    'Health Assessments': 'healthAssessments',
    'Medical Devices': 'medicalDevices',
  },

  attributeFormatPreferences: {
    test: ['text', 'code', 'display', 'name', 'value'],
    resultUnit: ['text', 'code', 'display', 'value'],
    resultValue: ['text', 'value', 'code', 'display'],
    specimenType: ['text', 'code', 'display', 'value'],
    performedDateTime: ['text', 'value', 'display'],
    resultStatus: ['text', 'code', 'display', 'value'],
  },

  genericPreferredFields: ['text', 'value', 'code', 'display', 'name', 'id', 'label', 'description'],

  statusNormalization: {
    pass: 'Pass',
    passed: 'Pass',
    fail: 'Fail',
    failed: 'Fail',
    skip: 'Skip',
    skipped: 'Skip',
  },

  fallbackAssessmentClass: 'Batch Row',
  fallbackAssessmentAttribute: 'Row Processing',
  fallbackAssessmentName: 'ScoringUnavailable',
  fallbackAssessmentStatus: 'Skip',
};

const DEFAULT_BATCH_ROW_CONCURRENCY = 20;
const MAX_BATCH_ROW_CONCURRENCY = 30;

function loadBatchConfig() {
  try {
    if (process.env.BATCH_CONFIG_JSON) {
      const parsed = JSON.parse(process.env.BATCH_CONFIG_JSON);
      return {...DEFAULT_BATCH_CONFIG, ...parsed};
    }
  } catch (err) {
    console.warn('Failed to parse BATCH_CONFIG_JSON, using defaults:', err.message);
  }
  return DEFAULT_BATCH_CONFIG;
}

function loadBatchConcurrency() {
  const parsedConcurrency = Number.parseInt(process.env.BATCH_ROW_CONCURRENCY || `${DEFAULT_BATCH_ROW_CONCURRENCY}`, 10);
  return Math.min(
    MAX_BATCH_ROW_CONCURRENCY,
    Math.max(Number.isNaN(parsedConcurrency) ? DEFAULT_BATCH_ROW_CONCURRENCY : parsedConcurrency, 1)
  );
}

const BATCH_CONFIG = loadBatchConfig();
const BATCH_ROW_CONCURRENCY = loadBatchConcurrency();

const os = require('os');

const DEFAULT_CONCURRENCY = Math.max(2, os.cpus().length);

module.exports = {
  DEFAULT_BATCH_CONFIG,
  DEFAULT_BATCH_ROW_CONCURRENCY,
  MAX_BATCH_ROW_CONCURRENCY,
  BATCH_CONFIG,
  BATCH_ROW_CONCURRENCY,
  loadBatchConfig,
  loadBatchConcurrency,
};
