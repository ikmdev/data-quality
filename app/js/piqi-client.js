// Shared JavaScript for PIQI client pages.
// Each page must define buildRequestBody() and clearForm() locally.

// ---------------------------------------------------------------------------
// Utility helpers
// ---------------------------------------------------------------------------

function escapeHtml(text) {
    if (!text) return '';
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.replace(/[&<>"']/g, m => map[m]);
}

function syntaxHighlightJson(json) {
    if (!json) return '';

    json = escapeHtml(json);

    json = json.replace(/&quot;([^&]*?)&quot;:\s?/g, '<span class="json-key">&quot;$1&quot;</span>: ');
    json = json.replace(/:\s?&quot;([^&]*?)&quot;/g, ': <span class="json-string">&quot;$1&quot;</span>');
    json = json.replace(/:\s?([0-9.]+)([,\n])/g, ': <span class="json-number">$1</span>$2');
    json = json.replace(/:\s?(true|false)([,\n])/g, ': <span class="json-boolean">$1</span>$2');
    json = json.replace(/:\s?(null)([,\n])/g, ': <span class="json-null">$1</span>$2');
    json = json.replace(/([\[\]{}:,])/g, '<span class="json-punctuation">$1</span>');

    return json;
}

function openValueModal(content) {
    const modal = document.getElementById('valueModal');
    const jsonDisplay = document.getElementById('jsonDisplay');

    if (!modal) {
        console.error('Modal element not found');
        return;
    }

    // Try to parse and pretty-print the content
    let formatted = content;
    try {
        const parsed = JSON.parse(content);
        formatted = JSON.stringify(parsed, null, 2);
    } catch (e) {
        // Not JSON, use as-is
        formatted = content;
    }

    const displayContent = syntaxHighlightJson(formatted);
    jsonDisplay.innerHTML = displayContent;

    modal.classList.add('show');
    modal.focus();
}

function closeValueModal() {
    const modal = document.getElementById('valueModal');
    if (modal) {
        modal.classList.remove('show');
    }
}

function copyToClipboard() {
    const jsonDisplay = document.getElementById('jsonDisplay');
    const text = jsonDisplay.textContent;

    navigator.clipboard.writeText(text).then(() => {
        const copyBtn = document.getElementById('modalCopyBtn');
        const origText = copyBtn.textContent;
        copyBtn.textContent = '✓ Copied!';
        copyBtn.classList.add('copied');

        setTimeout(() => {
            copyBtn.textContent = origText;
            copyBtn.classList.remove('copied');
        }, 2000);
    }).catch(err => {
        alert('Failed to copy: ' + err);
    });
}

// ---------------------------------------------------------------------------
// Dropdown population from piqi-config.js global
// ---------------------------------------------------------------------------

function loadDropdownOptions() {
    if (typeof PIQI_CONFIG === 'undefined') {
        console.error('PIQI_CONFIG not found. Ensure piqi-config.js is loaded before piqi-client.js.');
        return;
    }

    const modelSelect = document.getElementById('piqiModelMnemonic');
    const rubricSelect = document.getElementById('evaluationRubricMnemonic');

    if (modelSelect && Array.isArray(PIQI_CONFIG.piqiModelMnemonics)) {
        modelSelect.innerHTML = '';
        PIQI_CONFIG.piqiModelMnemonics.forEach(val => {
            const opt = document.createElement('option');
            opt.value = val;
            opt.textContent = val;
            modelSelect.appendChild(opt);
        });
    }

    if (rubricSelect && Array.isArray(PIQI_CONFIG.evaluationRubricMnemonics)) {
        rubricSelect.innerHTML = '';
        PIQI_CONFIG.evaluationRubricMnemonics.forEach(val => {
            const opt = document.createElement('option');
            opt.value = val;
            opt.textContent = val;
            rubricSelect.appendChild(opt);
        });
    }
}

// Close modal when clicking overlay
document.addEventListener('DOMContentLoaded', function () {
    loadDropdownOptions();

    const modal = document.getElementById('valueModal');
    if (modal) {
        modal.addEventListener('click', function (e) {
            if (e.target === modal) {
                closeValueModal();
            }
        });
    }
});

// Close modal on Escape key
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        closeValueModal();
    }
});

// Toggle evaluation section visibility
function toggleEvaluation(evalId) {
    const content = document.getElementById(evalId);
    const toggle = document.getElementById(evalId + '-toggle');

    if (content.style.display === 'none') {
        content.style.display = 'block';
        toggle.textContent = '−';
    } else {
        content.style.display = 'none';
        toggle.textContent = '+';
    }
}

// Expand all evaluation sections
function expandAllEvaluations(count) {
    for (let i = 0; i < count; i++) {
        const content = document.getElementById('eval-' + i);
        const toggle = document.getElementById('eval-' + i + '-toggle');
        if (content) {
            content.style.display = 'block';
            toggle.textContent = '−';
        }
    }
}

// Collapse all evaluation sections
function collapseAllEvaluations(count) {
    for (let i = 0; i < count; i++) {
        const content = document.getElementById('eval-' + i);
        const toggle = document.getElementById('eval-' + i + '-toggle');
        if (content) {
            content.style.display = 'none';
            toggle.textContent = '+';
        }
    }
}

// ---------------------------------------------------------------------------
// JSON formatting helpers
// ---------------------------------------------------------------------------

function hasFailedStatus(obj) {
    if (typeof obj !== 'object' || obj === null) return false;
    return obj.status === 'Failed';
}

function formatJSONWithFailedHighlight(obj, indent = 0) {
    const indentStr = '  '.repeat(indent);
    const nextIndentStr = '  '.repeat(indent + 1);

    if (obj === null) {
        return 'null';
    }

    if (typeof obj !== 'object') {
        if (typeof obj === 'string') {
            return '"' + obj + '"';
        }
        return String(obj);
    }

    if (Array.isArray(obj)) {
        if (obj.length === 0) return '[]';

        let result = '[\n';
        obj.forEach((item, index) => {
            const isFailed = hasFailedStatus(item);

            if (isFailed) {
                result += nextIndentStr + '<span class="failed-assessment-item">';
            } else {
                result += nextIndentStr;
            }

            result += formatJSONWithFailedHighlight(item, indent + 1);

            if (isFailed) {
                result += '</span>';
            }

            if (index < obj.length - 1) result += ',';
            result += '\n';
        });
        result += indentStr + ']';
        return result;
    }

    // Object
    const keys = Object.keys(obj);
    if (keys.length === 0) return '{}';

    let result = '{\n';
    keys.forEach((key, index) => {
        result += nextIndentStr + '"' + key + '": ';
        result += formatJSONWithFailedHighlight(obj[key], indent + 1);
        if (index < keys.length - 1) result += ',';
        result += '\n';
    });
    result += indentStr + '}';
    return result;
}

// ---------------------------------------------------------------------------
// Request preview
// ---------------------------------------------------------------------------

async function previewRequest() {
    const previewSection = document.getElementById('previewSection');
    const previewContent = document.getElementById('previewContent');

    try {
        const messageDataStr = document.getElementById('messageData').value;
        let messageDataParsed;

        try {
            messageDataParsed = JSON.parse(messageDataStr);
        } catch (parseError) {
            throw new Error('Invalid JSON in Message Data field: ' + parseError.message);
        }

        // Check if it's an array
        const messages = Array.isArray(messageDataParsed) ? messageDataParsed : [messageDataParsed];

        let previewHTML = '';

        messages.forEach((msg, index) => {
            const requestBody = {
                dataProviderID: document.getElementById('dataProviderID').value,
                dataSourceID: document.getElementById('dataSourceID').value,
                messageID: msg.messageID || msg.messageId || '',
                piqiModelMnemonic: document.getElementById('piqiModelMnemonic').value,
                evaluationRubricMnemonic: document.getElementById('evaluationRubricMnemonic').value,
                messageData: JSON.stringify(msg)
            };

            previewHTML += `<div style="margin-bottom: 20px; padding: 15px; border: 1px solid #ddd; border-radius: 5px;">`;
            previewHTML += `<h4 style="margin-top: 0;">Evaluation ${index + 1} of ${messages.length}</h4>`;
            previewHTML += `<pre style="background: #f5f5f5; padding: 10px; border-radius: 4px; overflow-x: auto;">${JSON.stringify(requestBody, null, 2)}</pre>`;
            previewHTML += `</div>`;
        });

        previewContent.innerHTML = previewHTML;
        previewSection.classList.add('show');
        previewSection.scrollIntoView({behavior: 'smooth', block: 'nearest'});

    } catch (error) {
        previewContent.innerHTML = `<div style="color: #d32f2f; padding: 10px;">Error: ${error.message}</div>`;
        previewSection.classList.add('show');
    }
}

// ---------------------------------------------------------------------------
// Response formatting
// ---------------------------------------------------------------------------

function getScoreBadgeClass(score) {
    if (score >= 80) return 'score-high';
    if (score >= 50) return 'score-medium';
    return 'score-low';
}

function formatMessageResults(messageResults) {
    if (!messageResults) return '';

    let html = '<div class="message-results-card"><h3>Message Results</h3>';
    html += '<div class="summary-grid">';

    html += '<div class="summary-item">';
    html += '<div class="summary-label">Denominator</div>';
    html += '<div class="summary-value">' + (messageResults.denominator !== undefined ? messageResults.denominator : 'N/A') + '</div>';
    html += '</div>';

    html += '<div class="summary-item">';
    html += '<div class="summary-label">Numerator</div>';
    html += '<div class="summary-value">' + (messageResults.numerator !== undefined ? messageResults.numerator : 'N/A') + '</div>';
    html += '</div>';

    html += '<div class="summary-item">';
    html += '<div class="summary-label">PIQI Score</div>';
    if (messageResults.piqiScore !== undefined && typeof messageResults.piqiScore === 'number') {
        const scoreClass = getScoreBadgeClass(messageResults.piqiScore);
        html += '<div class="summary-value"><span class="score-badge ' + scoreClass + '">' + messageResults.piqiScore.toFixed(2) + '%</span></div>';
    } else {
        html += '<div class="summary-value">' + (messageResults.piqiScore !== undefined ? messageResults.piqiScore : 'N/A') + '</div>';
    }
    html += '</div>';

    html += '<div class="summary-item">';
    html += '<div class="summary-label">Critical Failure Count</div>';
    html += '<div class="summary-value">' + (messageResults.criticalFailureCount !== undefined ? messageResults.criticalFailureCount : 'N/A') + '</div>';
    html += '</div>';

    html += '<div class="summary-item">';
    html += '<div class="summary-label">Weighted Denominator</div>';
    html += '<div class="summary-value">' + (messageResults.weightedDenominator !== undefined ? messageResults.weightedDenominator : 'N/A') + '</div>';
    html += '</div>';

    html += '<div class="summary-item">';
    html += '<div class="summary-label">Weighted Numerator</div>';
    html += '<div class="summary-value">' + (messageResults.weightedNumerator !== undefined ? messageResults.weightedNumerator : 'N/A') + '</div>';
    html += '</div>';

    html += '<div class="summary-item">';
    html += '<div class="summary-label">Weighted PIQI Score</div>';
    if (messageResults.weightedPIQIScore !== undefined && typeof messageResults.weightedPIQIScore === 'number') {
        const scoreClass = getScoreBadgeClass(messageResults.weightedPIQIScore);
        html += '<div class="summary-value"><span class="score-badge ' + scoreClass + '">' + messageResults.weightedPIQIScore.toFixed(2) + '%</span></div>';
    } else {
        html += '<div class="summary-value">' + (messageResults.weightedPIQIScore !== undefined ? messageResults.weightedPIQIScore : 'N/A') + '</div>';
    }
    html += '</div>';

    html += '</div>';
    html += '</div>';
    return html;
}

async function formatDataClassResults(runId, dataClassResults, auditedMessage) {
    if (!dataClassResults || !Array.isArray(dataClassResults) || dataClassResults.length === 0) {
        return '';
    }

    let parsedAuditedMessage;
    try {
        if (typeof auditedMessage === 'string') {
            parsedAuditedMessage = JSON.parse(auditedMessage);
        } else {
            parsedAuditedMessage = auditedMessage;
        }
    } catch (e) {
        console.error('Error parsing auditedMessage:', e);
        return '<p class="error">Unable to parse audited message for detailed assessment items.</p>';
    }

    const activeResults = dataClassResults.filter(result => result.instanceCount > 0);

    if (activeResults.length === 0) {
        return '';
    }

    let html = '<h2>Data Class Results Summary</h2>';

    const dataClassPaths = {
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
        'Medical Devices': 'medicalDevices'
    };

    for (const result of activeResults) {
        const className = result.dataClassName || 'Unknown';
        const dataPath = dataClassPaths[className];

        html += '<div class="data-class-card">';
        html += '<div class="data-class-header">' + className + '</div>';

        const assessmentItems = extractAssessmentItems(parsedAuditedMessage, dataPath);

        if (assessmentItems.length > 0) {
            html += '<div style="overflow-x: auto;">';
            html += '<table class="assessment-table">';
            html += '<thead>';
            html += '<tr>';
            html += '<th>Attribute</th>';
            html += '<th>Value</th>';
            html += '<th>Assessment</th>';
            html += '<th>Status</th>';
            html += '<th>Reason</th>';
            html += '<th>Effect</th>';
            html += '</tr>';
            html += '</thead>';
            html += '<tbody>';

            for (const item of assessmentItems) {
                const statusClass = getStatusClass(item.status);
                const attrValueDisplay = item.attributeValue || 'N/A';
                html += '<tr>';
                html += '<td class="attr-name">' + (item.attributeName || 'N/A') + '</td>';
                if (attrValueDisplay === 'N/A') {
                    html += '<td class="attr-value">N/A</td>';
                } else {
                    html += '<td class="attr-value" onclick=\'openValueModal(' + JSON.stringify(attrValueDisplay) + ')\' style="cursor: pointer; text-decoration: underline;" title="Click to view full content">' + attrValueDisplay + '</td>';
                }
                html += '<td class="assessment-desc">' + (item.assessment || 'N/A') + '</td>';
                html += '<td><span class="status-badge ' + statusClass + '">' + (item.status || 'N/A') + '</span></td>';
                html += '<td class="reason-text">' + (item.reason || '-') + '</td>';
                html += '<td class="effect-text">' + (item.effect || 'N/A') + '</td>';
                html += '</tr>';

                // Write Evaluation Result
                if (runId) {
                    const savedData = await saveEvaluationResult(
                        runId,
                        result.dataClassName,
                        item.attributeName,
                        item.attributeValue,
                        item.assessment,
                        item.status,
                        item.reason,
                        item.effect);

                    if (savedData) {
                        // You can now access savedData.id or savedData.created_at if you need them for the UI
                        console.log("Successfully recorded attribute check at", savedData.created_at);
                    }
                }
            }

            html += '</tbody>';
            html += '</table>';
            html += '</div>';
        } else {
            html += '<p class="info">No assessment items found for this data class.</p>';
        }

        html += '</div>';
    }

    return html;
}

function extractAssessmentItems(parsedMessage, dataPath) {
    const items = [];

    if (!parsedMessage.patient || !dataPath || !parsedMessage.patient[dataPath]) {
        return items;
    }

    const dataArray = parsedMessage.patient[dataPath];

    if (!Array.isArray(dataArray)) {
        return items;
    }

    dataArray.forEach(element => {
        for (const key in element) {
            if (element.hasOwnProperty(key) && element[key]) {
                const attribute = element[key];

                if (attribute.attributeAudit && attribute.attributeAudit.assessmentItems) {
                    const assessmentItems = attribute.attributeAudit.assessmentItems;

                    if (Array.isArray(assessmentItems)) {
                        assessmentItems.forEach(item => {
                            // Format the attribute value from the data element for display
                            let displayValue = 'N/A';
                            if (attribute.data !== undefined && attribute.data !== null) {
                                if (typeof attribute.data === 'object') {
                                    displayValue = JSON.stringify(attribute.data);
                                } else if (typeof attribute.data === 'string') {
                                    displayValue = attribute.data.length > 100 ? attribute.data.substring(0, 100) + '...' : attribute.data;
                                } else {
                                    displayValue = String(attribute.data);
                                }
                            }
                            items.push({
                                attributeName: item.attributeName || key,
                                attributeMnemonic: item.attributeMnemonic,
                                attributeValue: displayValue,
                                assessment: item.assessment,
                                status: item.status,
                                reason: item.reason,
                                effect: item.effect
                            });
                        });
                    }
                }
            }
        }
    });

    return items;
}

function getStatusClass(status) {
    if (!status) return 'status-unknown';
    const statusLower = status.toLowerCase();
    if (statusLower === 'passed') return 'status-passed';
    if (statusLower === 'failed') return 'status-failed';
    if (statusLower === 'skipped') return 'status-skipped';
    return 'status-unknown';
}

function formatAuditedMessage(auditedMessage) {
    if (!auditedMessage) return '';

    let html = '<div class="audited-message-section">';
    html += '<div class="audited-message-header">';
    html += '<span>Audited Message</span>';
    html += '<button class="toggle-audited-btn" onclick="toggleAuditedMessage()">Collapse</button>';
    html += '</div>';
    html += '<div class="audited-message-content" id="auditedMessageContent">';
    html += '<div class="audited-message-json">';

    let parsedData;
    if (typeof auditedMessage === 'object') {
        parsedData = auditedMessage;
    } else if (typeof auditedMessage === 'string') {
        try {
            parsedData = JSON.parse(auditedMessage);
        } catch (e) {
            html += auditedMessage;
            html += '</div></div></div>';
            return html;
        }
    } else {
        html += String(auditedMessage);
        html += '</div></div></div>';
        return html;
    }

    html += formatJSONWithFailedHighlight(parsedData, 0);

    html += '</div></div></div>';
    return html;
}

function displayFormattedResponse(responseData) {
    const container = document.getElementById('formattedResponseContainer');
    let html = '<div class="formatted-response">';

    if (responseData.scoringData) {
        if (responseData.scoringData.messageResults) {
            html += formatMessageResults(responseData.scoringData.messageResults);
        }
        if (responseData.scoringData.dataClassResults) {
            html += formatDataClassResults(responseData.scoringData.dataClassResults, responseData.auditedMessage);
        }
    }

    if (responseData.auditedMessage) {
        html += formatAuditedMessage(responseData.auditedMessage);
    }

    html += '</div>';
    container.innerHTML = html;
}

// ---------------------------------------------------------------------------
// Toggle helpers
// ---------------------------------------------------------------------------

function toggleRawResponse() {
    const rawResponse = document.getElementById('rawResponse');
    const btn = document.getElementById('toggleRawBtn');

    if (rawResponse.classList.contains('show')) {
        rawResponse.classList.remove('show');
        btn.textContent = 'Show Raw JSON Response';
    } else {
        rawResponse.classList.add('show');
        btn.textContent = 'Hide Raw JSON Response';
    }
}

function toggleAuditedMessage() {
    const content = document.getElementById('auditedMessageContent');
    const btn = event.target;

    if (content.classList.contains('collapsed')) {
        content.classList.remove('collapsed');
        btn.textContent = 'Collapse';
    } else {
        content.classList.add('collapsed');
        btn.textContent = 'Expand';
    }
}

// ---------------------------------------------------------------------------
// Form submission (shared)
// ---------------------------------------------------------------------------
document.getElementById('apiForm').addEventListener('submit', async function (e) {
    e.preventDefault();

    const submitBtn = e.target.querySelector('.submit-btn');
    const loading = document.getElementById('loading');
    const responseSection = document.getElementById('responseSection');
    const responseHeader = document.getElementById('responseHeader');
    const responseContent = document.getElementById('responseContent');
    const formattedContainer = document.getElementById('formattedResponseContainer');

    submitBtn.disabled = true;
    loading.classList.add('show');
    responseSection.classList.remove('show');

    try {
        const piqiUrl = document.getElementById('piqiUrl').value;
        const messageDataStr = document.getElementById('messageData').value;

        let messageDataParsed;
        try {
            messageDataParsed = JSON.parse(messageDataStr);
        } catch (parseError) {
            throw new Error('Invalid JSON in Message Data field: ' + parseError.message);
        }

        // Check if it's an array
        const messages = Array.isArray(messageDataParsed) ? messageDataParsed : [messageDataParsed];

        let successCount = 0;
        let failCount = 0;
        let allResults = [];
        let formattedHTML = '';

        // Write Evaluation Run
        let runId = await startEvaluationRun(
            document.getElementById('runName').value,
            document.getElementById('piqiModelMnemonic').value,
            document.getElementById('evaluationRubricMnemonic').value,
            messages.length);

        // Process each message individually
        for (let i = 0; i < messages.length; i++) {
            const msg = messages[i];
            const requestBody = {
                dataProviderID: document.getElementById('dataProviderID').value,
                dataSourceID: document.getElementById('dataSourceID').value,
                messageID: msg.messageID || msg.messageId || '',
                piqiModelMnemonic: document.getElementById('piqiModelMnemonic').value,
                evaluationRubricMnemonic: document.getElementById('evaluationRubricMnemonic').value,
                messageData: JSON.stringify(msg)
            };

            try {
                console.log(`Sending evaluation ${i + 1} of ${messages.length} to:`, piqiUrl);

                // Send data to PIQI Engine for evaluation
                const response = await fetch(piqiUrl, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(requestBody)
                });

                let responseData;
                const contentType = response.headers.get('content-type');
                if (contentType && contentType.includes('application/json')) {
                    responseData = await response.json();
                } else {
                    responseData = await response.text();
                }

                if (response.ok) {
                    successCount++;

                    // Build collapsible formatted HTML for this evaluation
                    formattedHTML += `<div style="margin-bottom: 20px; border: 2px solid #4caf50; border-radius: 8px; background: #f1f8f4;">`;
                    formattedHTML += `<div style="padding: 15px; cursor: pointer; display: flex; justify-content: space-between; align-items: center;" onclick="toggleEvaluation('eval-${i}')">`;
                    formattedHTML += `<h3 style="margin: 0; color: #2e7d32;">✓ Evaluation ${i + 1} of ${messages.length} - Success (Message ID: ${requestBody.messageID || 'N/A'})</h3>`;
                    formattedHTML += `<button type="button" style="background: none; border: none; font-size: 24px; cursor: pointer;" id="eval-${i}-toggle">+</button>`;
                    formattedHTML += `</div>`;
                    formattedHTML += `<div id="eval-${i}" style="display: none; padding: 0 15px 15px 15px;">`;

                    if (typeof responseData === 'object') {
                        formattedHTML += '<div class="formatted-response">';
                        if (responseData.scoringData) {
                            if (responseData.scoringData.messageResults) {
                                formattedHTML += formatMessageResults(responseData.scoringData.messageResults);
                            }
                            if (responseData.scoringData.dataClassResults) {
                                formattedHTML += await formatDataClassResults(runId, responseData.scoringData.dataClassResults, responseData.auditedMessage);
                            }
                        }
                        if (responseData.auditedMessage) {
                            formattedHTML += formatAuditedMessage(responseData.auditedMessage);
                        }
                        formattedHTML += '</div>';
                    } else {
                        formattedHTML += `<p>Response is not JSON format</p>`;
                    }

                    formattedHTML += `</div></div>`;

                    allResults.push({
                        index: i + 1,
                        success: true,
                        messageID: requestBody.messageID,
                        data: responseData
                    });

                } else {
                    failCount++;

                    formattedHTML += `<div style="margin-bottom: 20px; border: 2px solid #f44336; border-radius: 8px; background: #ffebee;">`;
                    formattedHTML += `<div style="padding: 15px; cursor: pointer; display: flex; justify-content: space-between; align-items: center;" onclick="toggleEvaluation('eval-${i}')">`;
                    formattedHTML += `<h3 style="margin: 0; color: #c62828;">✗ Evaluation ${i + 1} of ${messages.length} - Failed (Status: ${response.status}, Message ID: ${requestBody.messageID || 'N/A'})</h3>`;
                    formattedHTML += `<button type="button" style="background: none; border: none; font-size: 24px; cursor: pointer;" id="eval-${i}-toggle">+</button>`;
                    formattedHTML += `</div>`;
                    formattedHTML += `<div id="eval-${i}" style="padding: 0 15px 15px 15px;">`;
                    formattedHTML += `<pre style="background: #fff; padding: 10px; border-radius: 4px; overflow-x: auto;">${typeof responseData === 'object' ? JSON.stringify(responseData, null, 2) : responseData}</pre>`;
                    formattedHTML += `</div></div>`;

                    allResults.push({
                        index: i + 1,
                        success: false,
                        messageID: requestBody.messageID,
                        error: typeof responseData === 'object' ? JSON.stringify(responseData) : responseData
                    });
                }

            } catch (error) {
                failCount++;

                formattedHTML += `<div style="margin-bottom: 20px; border: 2px solid #ff9800; border-radius: 8px; background: #fff3e0;">`;
                formattedHTML += `<div style="padding: 15px; cursor: pointer; display: flex; justify-content: space-between; align-items: center;" onclick="toggleEvaluation('eval-${i}')">`;
                formattedHTML += `<h3 style="margin: 0; color: #e65100;">⚠ Evaluation ${i + 1} of ${messages.length} - Error (Message ID: ${requestBody.messageID || 'N/A'})</h3>`;
                formattedHTML += `<button type="button" style="background: none; border: none; font-size: 24px; cursor: pointer;" id="eval-${i}-toggle">+</button>`;
                formattedHTML += `</div>`;
                formattedHTML += `<div id="eval-${i}" style="padding: 0 15px 15px 15px;">`;
                formattedHTML += `<p><strong>Error:</strong> ${error.message}</p>`;
                formattedHTML += `</div></div>`;

                allResults.push({
                    index: i + 1,
                    success: false,
                    messageID: requestBody.messageID,
                    error: error.message
                });
            }
        }

        // Show overall results
        responseSection.classList.add('show');
        responseHeader.innerHTML = `<span class="success">✓ Completed ${messages.length} evaluation(s): ${successCount} succeeded, ${failCount} failed</span>`;

        if (successCount > 0) {
            responseHeader.innerHTML += ' | ✓ Saved to Database';
        }

        // Add collapse/expand all buttons
        const toggleButtonsHTML = `    <div style="margin: 15px 0; display: flex; gap: 10px;">
        <button type="button" onclick="expandAllEvaluations(${messages.length})" style="padding: 8px 16px; background: #4caf50; color: white; border: none; border-radius: 4px; cursor: pointer;">Expand All</button>
        <button type="button" onclick="collapseAllEvaluations(${messages.length})" style="padding: 8px 16px; background: #757575; color: white; border: none; border-radius: 4px; cursor: pointer;">Collapse All</button>
    </div>`;

        formattedContainer.innerHTML = toggleButtonsHTML + formattedHTML;
        responseContent.textContent = JSON.stringify(allResults, null, 2);

        responseSection.scrollIntoView({behavior: 'smooth', block: 'nearest'});

        const status = failCount > 0 ? 'FAILED' : 'COMPLETE';
        if (runId) {
            await finishEvaluationRun(
                runId,
                status,
                successCount,
                failCount);
        }


    } catch (error) {
        responseSection.classList.add('show');
        responseHeader.innerHTML = '<span class="error">✗ Error</span>';
        formattedContainer.innerHTML = '';
        responseContent.textContent = error.message;
        responseSection.scrollIntoView({behavior: 'smooth', block: 'nearest'});
    } finally {
        submitBtn.disabled = false;
        loading.classList.remove('show');
    }
});

// status = 'IN_PROGRESS' | 'COMPLETED' | 'FAILED'
async function startEvaluationRun(name, modelMnemonic, rubricMnemonic, totalEvaluations) {
    try {
        const saveRunResult = await fetch('http://localhost/postgrest/piqi_evaluation_run', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Prefer': 'return=representation' // This tells Postgres to hand the row back
            },
            body: JSON.stringify({
                run_name: name,
                piqi_model_mnemonic: modelMnemonic,
                evaluation_rubric_mnemonic: rubricMnemonic,
                total_evaluations: totalEvaluations,
                status: 'IN_PROGRESS'
            })
        });

        if (!saveRunResult.ok) {
            const error = await saveRunResult.json();
            console.error("Run Insert Failed:", error);
            return null; // Return null so your app knows to stop
        }

        // Extract the newly created row
        const [newRun] = await saveRunResult.json();
        console.log("Success! Run created with ID:", newRun.id);

        // Return the ID directly to whatever called this function
        return newRun.id;

    } catch (error) {
        console.error("Network or API error:", error);
        return null;
    }
}

async function finishEvaluationRun(runId, finalStatus, completedCount = 0, failedCount = 0) {
    try {
        // Notice the URL syntax: ?id=eq.123
        // This is PostgREST's way of writing "WHERE id = 123"
        const updateResult = await fetch(`http://localhost/postgrest/piqi_evaluation_run?id=eq.${runId}`, {
            method: 'PATCH',
            headers: {
                'Content-Type': 'application/json',
                'Prefer': 'return=representation' // Tell Postgres to hand back the updated row
            },
            body: JSON.stringify({
                status: finalStatus,
                total_completed: completedCount,
                total_failed: failedCount,
                // Automatically stamp the finish time using the browser's ISO clock
                completed_at: new Date().toISOString()
            })
        });

        if (!updateResult.ok) {
            const error = await updateResult.json();
            console.error(`Failed to update Run ID ${runId}:`, error);
            return null;
        }

        // Extract the fully updated row from the database response
        const [updatedRun] = await updateResult.json();
        console.log(`Success! Run ${runId} finalized with status: ${updatedRun.status}`);

        return updatedRun;

    } catch (error) {
        console.error("Network or API error during update:", error);
        return null;
    }
}

async function saveEvaluationResult(runId, dataClass, attributeName, attributeValue, assessment, status, reason, effect) {
    try {
        const response = await fetch('http://localhost/postgrest/piqi_evaluation_results', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Prefer': 'return=representation'
            },
            body: JSON.stringify({
                run_id: runId,                   // REQUIRED
                data_class: dataClass,           // REQUIRED
                attribute_name: attributeName,   // REQUIRED
                attribute_value: attributeValue,
                assessment: assessment,
                status: status,
                reason: reason,
                effect: effect
            })
        });

        if (!response.ok) {
            const error = await response.json();
            console.error("Database Insert Rejected:", error);
            return null;
        }

        const [savedRow] = await response.json();
        console.log(`Result saved successfully with ID: ${savedRow.id}`);

        return savedRow;

    } catch (error) {
        console.error("Network or API error while saving result:", error);
        return null;
    }
}