const express = require('express');
const router = express.Router();
const { generatePass, getMyPasses } = require('../controllers/passController');
const { verifyFirebaseToken, requireRegisteredUser } = require('../middleware/auth');
const Pass = require('../models/Pass'); // Adjust path to your Pass model

router.use(verifyFirebaseToken, requireRegisteredUser);

router.post('/generate', generatePass);
router.get('/my-passes', getMyPasses);

// Pass Scan & Validation for Guards/Admins
router.post('/scan', async (req, res) => {
  try {
    const { passId } = req.body;
    const pass = await Pass.findOne({ _id: passId });

    if (!pass) {
      return res.status(404).json({ valid: false, message: 'Pass not found' });
    }

    const isExpired = new Date() > new Date(pass.expiresAt);
    if (isExpired || pass.status !== 'ACTIVE') {
      return res.status(200).json({
        valid: false,
        message: isExpired ? 'Pass Has Expired' : 'Pass Inactive',
        pass
      });
    }

    res.status(200).json({ valid: true, message: 'Pass Validated Successfully', pass });
  } catch (err) {
    res.status(500).json({ valid: false, message: 'Invalid Pass ID format' });
  }
});

module.exports = router;