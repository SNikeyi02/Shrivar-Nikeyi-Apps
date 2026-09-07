// middleware/auth.js
const admin = require('../config/firebase');
const User = require('../models/User');

const verifyFirebaseToken = async (req, res, next) => {
  const authHeader = req.headers.authorization;

  if (!authHeader || !authHeader.startsWith('Bearer ')) {
    return res.status(401).json({ message: 'Missing or malformed Authorization header' });
  }

  const token = authHeader.split('Bearer ')[1];

  try {
    const decodedToken = await admin.auth().verifyIdToken(token);
    req.user = decodedToken;

    // Attach MongoDB profile if it exists
    const dbUser = await User.findOne({ firebaseUid: decodedToken.uid });
    if (dbUser) {
      req.dbUser = dbUser;
    }

    next();
  } catch (error) {
    console.error('Token verification error:', error.message);
    return res.status(401).json({ message: 'Invalid or expired Firebase ID token' });
  }
};

// Middleware to ensure the user exists in MongoDB before accessing secured resource routes
const requireRegisteredUser = (req, res, next) => {
  if (!req.dbUser) {
    return res.status(403).json({ message: 'User profile not synchronized. Call /api/users/sync first.' });
  }
  next();
};

module.exports = { verifyFirebaseToken, requireRegisteredUser };