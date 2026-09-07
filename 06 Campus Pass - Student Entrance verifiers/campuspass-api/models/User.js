const mongoose = require('mongoose');

const userSchema = new mongoose.Schema({
  firebaseUid: { 
    type: String, 
    required: true, 
    unique: true 
  },
  email: { 
    type: String, 
    required: true 
  },
  displayName: { 
    type: String, 
    required: true 
  },
  role: { 
    type: String, 
    default: 'student' 
  }
}, { timestamps: true });

module.exports = mongoose.model('User', userSchema);