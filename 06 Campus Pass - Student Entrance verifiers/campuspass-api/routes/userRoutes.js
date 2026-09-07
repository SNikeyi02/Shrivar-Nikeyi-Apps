const express = require('express');
const router = express.Router();
const { verifyFirebaseToken } = require('../middleware/auth');
const User = require('../models/User');

// Register user profile in MongoDB after Firebase signup
router.post('/register', verifyFirebaseToken, async (req, res) => {
  try {
    const { name, role } = req.body;
    
    // Search using firebaseUid
    let user = await User.findOne({ firebaseUid: req.user.uid });

    if (user) {
      return res.status(400).json({ message: 'User already registered' });
    }

    // Map req.user and req.body to match database schema
    user = new User({
      firebaseUid: req.user.uid,
      email: req.user.email,
      displayName: name || 'Campus User',
      role: role ? role.toLowerCase() : 'student'
    });

    await user.save();
    res.status(201).json(user);
  } catch (err) {
    console.error('Registration Error:', err);
    res.status(500).json({ message: err.message });
  }
});

// Fetch current user details
router.get('/me', verifyFirebaseToken, async (req, res) => {
  try {
    const user = await User.findOne({ firebaseUid: req.user.uid });
    if (!user) return res.status(404).json({ message: 'User profile not found' });
    res.json(user);
  } catch (err) {
    res.status(500).json({ message: err.message });
  }
});

module.exports = router;