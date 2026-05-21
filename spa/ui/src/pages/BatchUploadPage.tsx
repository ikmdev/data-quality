import { useState, useRef } from 'react';
import { useBatchApi } from '../app/hooks/useBatchApi';

export default function BatchUploadPage() {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [provider, setProvider] = useState('Provider Alpha');
  const [source, setSource] = useState('Main EHR Feed');
  const [model, setModel] = useState('PAT_CLINICAL_V1');
  const [rubric, setRubric] = useState('USCDI_V3');
  const [concurrency, setConcurrency] = useState(20);

  const { isProcessing, errorMessage, summary, processingStatus, processFile, clearBatch } =
    useBatchApi();

  const handleFileSelect = (file: File) => {
    if (file.name.match(/\.(xlsx?|csv)$/i)) {
      setSelectedFile(file);
    } else {
      alert('Please select a valid Excel or CSV file');
    }
  };

  const handleDragOver = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    (e.currentTarget as HTMLDivElement).style.backgroundColor = '#e0e0e0';
  };

  const handleDragLeave = (e: React.DragEvent<HTMLDivElement>) => {
    (e.currentTarget as HTMLDivElement).style.backgroundColor = '';
  };

  const handleDrop = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    (e.currentTarget as HTMLDivElement).style.backgroundColor = '';
    if (e.dataTransfer.files.length > 0) {
      handleFileSelect(e.dataTransfer.files[0]);
    }
  };

  const handleClickDropZone = () => {
    fileInputRef.current?.click();
  };

  const handleFileInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      handleFileSelect(e.target.files[0]);
    }
  };

  const handleProcessBatch = async () => {
    if (!selectedFile) {
      alert('Please select a batch file first');
      return;
    }
    await processFile(selectedFile, provider, source, model, rubric, concurrency);
  };

  const handleClearBatch = () => {
    setSelectedFile(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
    clearBatch();
  };

  return (
    <div className="page-container">
      <h1>Batch Upload</h1>

      {/* Configuration Section */}
      <section className="form-section">
        <h2>Configuration</h2>
        <div className="form-grid" style={{ gridTemplateColumns: '1fr 1fr' }}>
          <div className="form-item">
            <label htmlFor="batch-provider">Provider ID</label>
            <input
              id="batch-provider"
              className="input"
              type="text"
              value={provider}
              onChange={(e) => setProvider(e.target.value)}
              placeholder="Provider ID"
            />
          </div>
          <div className="form-item">
            <label htmlFor="batch-source">Source ID</label>
            <input
              id="batch-source"
              className="input"
              type="text"
              value={source}
              onChange={(e) => setSource(e.target.value)}
              placeholder="Source ID"
            />
          </div>
          <div className="form-item">
            <label htmlFor="batch-model">Model Mnemonic</label>
            <input
              id="batch-model"
              className="input"
              type="text"
              value={model}
              onChange={(e) => setModel(e.target.value)}
              placeholder="PAT_CLINICAL_V1"
            />
          </div>
          <div className="form-item">
            <label htmlFor="batch-rubric">Evaluation Rubric</label>
            <input
              id="batch-rubric"
              className="input"
              type="text"
              value={rubric}
              onChange={(e) => setRubric(e.target.value)}
              placeholder="USCDI_V3"
            />
          </div>
          <div className="form-item">
            <label htmlFor="batch-concurrency">Concurrent Requests</label>
            <input
              id="batch-concurrency"
              className="input"
              type="number"
              min={1}
              max={50}
              value={concurrency}
              onChange={(e) => setConcurrency(Math.max(1, Math.min(50, Number(e.target.value))))}
            />
          </div>
        </div>
      </section>

      {/* File Upload Section */}
      <section className="form-section">
        <h2>Batch File Upload</h2>
        <div
          className="drop-zone"
          onDragOver={handleDragOver}
          onDragLeave={handleDragLeave}
          onDrop={handleDrop}
          onClick={handleClickDropZone}
          style={{ cursor: 'pointer' }}
        >
          {selectedFile ? (
            <div>
              <p style={{ fontSize: '16px', fontWeight: '600' }}>📄 {selectedFile.name}</p>
              <p style={{ fontSize: '12px', color: '#666' }}>Click to change file</p>
            </div>
          ) : (
            <div>
              <p>Drop Excel/CSV file here or click to choose</p>
              <p style={{ fontSize: '12px', color: '#666' }}>Supported: .xlsx, .xls, .csv</p>
            </div>
          )}
          <input
            ref={fileInputRef}
            type="file"
            accept=".xlsx,.xls,.csv"
            onChange={handleFileInputChange}
            style={{ display: 'none' }}
          />
        </div>

        {selectedFile && (
          <p style={{ fontSize: '12px', color: '#666', marginTop: '8px' }}>
            Ready to process: {selectedFile.name}
          </p>
        )}
      </section>

      {/* Action Buttons */}
      <div className="button-group">
        <button
          className="btn btn-primary"
          onClick={handleProcessBatch}
          disabled={isProcessing || !selectedFile}
        >
          {isProcessing ? 'Processing...' : 'Process Batch'}
        </button>
        <button
          className="btn btn-secondary"
          onClick={handleClearBatch}
          disabled={isProcessing}
        >
          Clear Batch
        </button>
      </div>

      {/* Error Message */}
      {errorMessage && (
        <div style={{ color: 'red', marginTop: '16px', padding: '12px', backgroundColor: '#fee2e2' }}>
          <strong>Error:</strong> {errorMessage}
        </div>
      )}

      {/* Processing Status */}
      {processingStatus && (
        <div style={{ marginTop: '16px', padding: '12px', backgroundColor: '#e3f2fd', color: '#1976d2' }}>
          {processingStatus}
        </div>
      )}

      {/* Summary Section */}
      {summary && (
        <section className="form-section" style={{ marginTop: '24px' }}>
          <h2>Batch Results Summary</h2>
          <div className="metric-grid">
            <div className="metric-box">
              <div style={{ fontSize: '12px', color: '#666' }}>Run ID</div>
              <div style={{ fontSize: '14px', fontWeight: '600', marginTop: '4px', wordBreak: 'break-all' }}>
                {summary.batchRunId}
              </div>
            </div>
            <div className="metric-box">
              <div style={{ fontSize: '12px', color: '#666' }}>Total Rows</div>
              <div style={{ fontSize: '20px', fontWeight: '700', marginTop: '4px' }}>
                {summary.totalRows}
              </div>
            </div>
            <div className="metric-box">
              <div style={{ fontSize: '12px', color: '#666' }}>Processed</div>
              <div style={{ fontSize: '20px', fontWeight: '700', marginTop: '4px' }}>
                {summary.processedRows}
              </div>
            </div>
            <div className="metric-box">
              <div style={{ fontSize: '12px', color: '#666' }}>Succeeded</div>
              <div style={{ fontSize: '20px', fontWeight: '700', color: '#059669', marginTop: '4px' }}>
                {summary.succeededRows}
              </div>
            </div>
            <div className="metric-box">
              <div style={{ fontSize: '12px', color: '#666' }}>Failed</div>
              <div style={{ fontSize: '20px', fontWeight: '700', color: '#dc2626', marginTop: '4px' }}>
                {summary.failedRows}
              </div>
            </div>
            <div className="metric-box">
              <div style={{ fontSize: '12px', color: '#666' }}>Avg Score</div>
              <div style={{ fontSize: '20px', fontWeight: '700', marginTop: '4px' }}>
                {summary.averageScore}
              </div>
            </div>
            <div className="metric-box">
              <div style={{ fontSize: '12px', color: '#666' }}>Success Rate</div>
              <div style={{ fontSize: '20px', fontWeight: '700', marginTop: '4px' }}>
                {summary.successRate}
              </div>
            </div>
            <div className="metric-box">
              <div style={{ fontSize: '12px', color: '#666' }}>Elapsed Time</div>
              <div style={{ fontSize: '16px', fontWeight: '600', marginTop: '4px' }}>
                {(summary.elapsedMs / 1000).toFixed(1)}s
              </div>
            </div>
          </div>

          <div className="metric-grid" style={{ marginTop: '12px' }}>
            <div className="metric-box" style={{ borderColor: '#a7f3d0', backgroundColor: '#f0fdf4' }}>
              <div style={{ fontSize: '12px', color: '#166534' }}>Passed Checks</div>
              <div style={{ fontSize: '20px', fontWeight: '700', color: '#15803d', marginTop: '4px' }}>
                {summary.classBreakdown.pass}
              </div>
            </div>
            <div className="metric-box" style={{ borderColor: '#fecaca', backgroundColor: '#fef2f2' }}>
              <div style={{ fontSize: '12px', color: '#991b1b' }}>Failed Checks</div>
              <div style={{ fontSize: '20px', fontWeight: '700', color: '#dc2626', marginTop: '4px' }}>
                {summary.classBreakdown.fail}
              </div>
            </div>
            <div className="metric-box" style={{ borderColor: '#fde68a', backgroundColor: '#fffbeb' }}>
              <div style={{ fontSize: '12px', color: '#92400e' }}>Skipped Checks</div>
              <div style={{ fontSize: '20px', fontWeight: '700', color: '#d97706', marginTop: '4px' }}>
                {summary.classBreakdown.skip}
              </div>
            </div>
          </div>

          <div style={{ marginTop: '12px', padding: '12px', borderRadius: '8px', backgroundColor: '#f8fafc', color: '#334155' }}>
            Detailed batch checks are saved to PostgreSQL only.
          </div>
        </section>
      )}
    </div>
  );
}
