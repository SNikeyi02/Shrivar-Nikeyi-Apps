const Pass = require('../models/Pass');
const crypto = require('crypto');

exports.generatePass = async (req, res) => {
  try {
    const userId = req.dbUser._id;
    const passCode = `CP-${crypto.randomBytes(4).toString('hex').toUpperCase()}`;
    const validUntil = new Date(Date.now() + 24 * 60 * 60 * 1000); // 24-hour expiration

    const pass = await Pass.create({
      user: userId,
      passCode,
      title: req.body.title || 'Standard Campus Pass',
      validUntil,
    });

    return res.status(201).json(pass);
  } catch (error) {
    console.error('Pass generation error:', error.message);
    return res.status(500).json({ message: 'Failed to generate campus pass' });
  }
};

exports.getMyPasses = async (req, res) => {
  try {
    const passes = await Pass.find({
      user: req.dbUser._id,
      status: 'active',
      validUntil: { $gt: new Date() },
    }).sort({ createdAt: -1 });

    return res.status(200).json(passes);
  } catch (error) {
    console.error('Fetch passes error:', error.message);
    return res.status(500).json({ message: 'Failed to retrieve passes' });
  }
};