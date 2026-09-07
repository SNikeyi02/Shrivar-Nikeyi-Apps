const mongoose = require('mongoose');

const passSchema = new mongoose.Schema(
  {
    user: {
      type: mongoose.Schema.Types.ObjectId,
      ref: 'User',
      required: true,
      index: true,
    },
    passCode: {
      type: String,
      required: true,
      unique: true,
      index: true,
    },
    title: {
      type: String,
      required: true,
      default: 'Standard Campus Pass',
    },
    status: {
      type: String,
      enum: ['active', 'used', 'expired', 'revoked'],
      default: 'active',
    },
    validUntil: {
      type: Date,
      required: true,
    },
  },
  { timestamps: true }
);

module.exports = mongoose.model('Pass', passSchema);