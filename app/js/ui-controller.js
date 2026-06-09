// app/js/ui-controller.js
import { initDuckDB, run } from './ingest.js';

// ---- DOM Elements ----
const els = {
    dataProviderID: document.getElementById('dataProviderID'),
    dataSourceID: document.getElementById('dataSourceID'),
    piqiUrl: document.getElementById('piqiUrl'),
    postgRestURL: document.getElementById('postgRestURL'),
    spreadsheetInput: document.getElementById('spreadsheetInput'),
    messageData: document.getElementById('messageData'),
    conversionStatus: document.getElementById('conversionStatus'),
    piqiModelMnemonic: document.getElementById('piqiModelMnemonic'),
    evaluationRubricMnemonic: document.getElementById('evaluationRubricMnemonic'),

    // Buttons
    btnConvert: document.getElementById('btnConvert'),
    btnClearPaste: document.getElementById('btnClearPaste'),
    btnClearForm: document.getElementById('btnClearForm'),
    // Note: btnSubmit and btnPreview handlers might already exist in your older piqi-client.js
    // If not, bind them here similarly.
};

// ---- UTILITY FUNCTIONS ----
function generateSessionID() {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    let sessionID = '';
    for (let i = 0; i < 32; i++) {
        sessionID += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return 'Session ID ' + sessionID;
}

function showStatus(message, type) {
    els.conversionStatus.textContent = message;
    els.conversionStatus.className = 'status-message show ' + type;
    setTimeout(() => {
        els.conversionStatus.classList.remove('show');
    }, 5000);
}

// ---- EVENT HANDLERS ----
async function handleConversion() {
    const input = els.spreadsheetInput.value.trim();
    if (!input) {
        showStatus('Please paste spreadsheet rows first.', 'error');
        return;
    }

    try {
        const lines = input.split('\n').filter(line => line.trim().length > 0);

        const sourceId = els.dataSourceID.value;
        const providerId = els.dataProviderID.value;

        // Route exclusively through DuckDB-WASM
        const messages = await run(lines, sourceId, providerId);

        els.messageData.value = JSON.stringify(messages, null, 2);
        showStatus(`✓ Successfully converted ${messages.length} spreadsheet row(s) to JSON using DuckDB!`, 'success');
        els.messageData.scrollIntoView({behavior: 'smooth', block: 'center'});

    } catch (error) {
        showStatus('DuckDB Error converting data: ' + error.message, 'error');
        console.error("Conversion failed:", error);
    }
}

function handleClearForm() {
    if (confirm('Are you sure you want to clear the form?')) {
        document.getElementById('apiForm').reset();
        els.messageData.value = '';
        document.getElementById('responseSection').classList.remove('show');
        document.getElementById('previewSection').classList.remove('show');
        els.spreadsheetInput.value = '';
        els.conversionStatus.classList.remove('show');
    }
}

// Global hook if piqi-client.js requires it for sending evaluation builds
window.buildRequestBody = function() {
    let messageDataParsed;
    try {
        messageDataParsed = JSON.parse(els.messageData.value);
    } catch (parseError) {
        throw new Error('Invalid JSON in Message Data field: ' + parseError.message);
    }
    const messageID = messageDataParsed.messageID || messageDataParsed.messageId || '';
    return {
        dataProviderID: els.dataProviderID.value,
        dataSourceID: els.dataSourceID.value,
        messageID: messageID,
        piqiModelMnemonic: els.piqiModelMnemonic.value,
        evaluationRubricMnemonic: els.evaluationRubricMnemonic.value,
        messageData: JSON.stringify(messageDataParsed)
    };
};


// ---- INIT / EVENT BINDING ----
document.addEventListener('DOMContentLoaded', () => {
    // Populate session data
    const sessionID = generateSessionID();
    els.dataProviderID.value = sessionID;
    els.dataSourceID.value = sessionID;
    els.piqiUrl.value = 'http://localhost//piqi/PIQI/ScoreAuditMessage';
    els.postgRestURL.value = 'http://localhost//postgres';
    els.messageData.value = '';

    // Pre-load DuckDB binaries silently in background
    initDuckDB().catch(err => console.error("Failed to pre-load DuckDB-WASM:", err));

    // Bind Button Click Events Explicitly!
    els.btnConvert.addEventListener('click', handleConversion);
    els.btnClearPaste.addEventListener('click', () => {
        els.spreadsheetInput.value = '';
        els.conversionStatus.classList.remove('show');
    });
    els.btnClearForm.addEventListener('click', handleClearForm);
});