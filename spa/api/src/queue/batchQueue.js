const Queue = require('bull');
const redis = require('redis');

// Create Redis client
const redisClient = redis.createClient({
  host: process.env.SPA_REDIS_URL || 'localhost',
  port: process.env.SPA_REDIS_PORT || 6379,
});

redisClient.on('error', (err) => console.error('Redis error:', err));
redisClient.on('connect', () => console.log('Redis connected'));

// Create Bull queue
const batchQueue = new Queue('batch-processing', {
  redis: {
    host: process.env.REDIS_URL || 'localhost',
    port: process.env.REDIS_PORT || 6379,
  },
});

// Configure queue
batchQueue.on('error', (err) => console.error('Queue error:', err));
batchQueue.on('stalled', (job) => console.warn(`Job ${job.id} stalled`));

module.exports = { batchQueue, redisClient };
