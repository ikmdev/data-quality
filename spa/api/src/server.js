const path = require('path');
require('dotenv').config({ path: path.resolve(__dirname, '../.env') });
const express = require('express');
const cors = require('cors');
const piqiRoutes = require('./routes/piqi');

// Initialize batch worker
require('./workers/batchWorker');

const app = express();
const PORT = Number(process.env.PORT || 5026);
const HOST = process.env.HOST || '0.0.0.0';
const bodyLimitMb = process.env.REQUEST_BODY_LIMIT_MB || '25';
const bodyLimit = `${bodyLimitMb}mb`;

const corsOptions = {
  origin(origin, callback) {
    // Allow browser clients from local Vite ports and non-browser/server requests.
    if (!origin || /^http:\/\/localhost:517\d$/.test(origin)) {
      callback(null, true);
      return;
    }
    callback(new Error(`CORS blocked origin: ${origin}`));
  },
};

app.use(cors(corsOptions));
app.use(express.json({ limit: bodyLimit }));
app.use(express.urlencoded({ limit: bodyLimit, extended: true }));

app.use('/PIQI', piqiRoutes);

app.get('/health', (_req, res) => {
  res.json({ status: 'ok' });
});

function startServer(port) {
  const server = app.listen(port, HOST, () => {
    console.log(`piqi-dq-api listening on http://localhost:${port}`);
  });

  server.on('error', (err) => {
    if (err && err.code === 'EADDRINUSE') {
      console.error(`Port ${port} is already in use. Stop the existing process and restart this service.`);
      process.exit(1);
    }

    console.error(`Failed to start server on port ${port}:`, err);
    process.exit(1);
  });
}

startServer(PORT);
