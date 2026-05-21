/**
 * Batch Worker - Main entry point
 * Re-exports from modular worker for backward compatibility
 */

const workerModule = require('./worker');
const batchConfigModule = require('./batchConfig');
const messageBuilderModule = require('./messageBuilder');
const assessmentExtractorModule = require('./assessmentExtractor');
const { batchQueue } = require('../queue/batchQueue');

module.exports = {
  ...workerModule,
  ...batchConfigModule,
  ...messageBuilderModule,
  ...assessmentExtractorModule,
  batchQueue,
};
