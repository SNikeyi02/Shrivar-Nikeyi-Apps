// controllers/userController.js
const User = require('../models/User');

exports.syncUser = async (req, res) => {
  try {
    const { uid, email, name } = req.user;

    if (!email) {
      return res.status(400).json({ message: 'User token must contain an email address.' });
    }

    // Atomic insert or update to prevent duplicate key race conditions
    const user = await User.findOneAndUpdate(
      { firebaseUid: uid },
      {
        $setOnInsert: {
          firebaseUid: uid,
          email: email.toLowerCase(),
          displayName: name || 'CampusPass User',
          role: 'attendee', 
        },
      },
      { new: true, upsert: true, runValidators: true }
    );

    return res.status(200).json(user);
  } catch (error) {
    console.error('User sync error:', error.message);
    return res.status(500).json({ message: 'Internal server error during profile sync' });
  }
};