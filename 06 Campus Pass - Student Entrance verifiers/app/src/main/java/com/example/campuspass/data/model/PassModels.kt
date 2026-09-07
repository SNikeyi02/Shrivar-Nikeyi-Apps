package com.example.campuspass.data.model

import com.google.gson.annotations.SerializedName

data class RegisterRequest(
    @SerializedName("name") val name: String,
    @SerializedName("role") val role: String
)

data class UserResponse(
    @SerializedName("uid") val uid: String,
    @SerializedName("email") val email: String,
    @SerializedName("name") val name: String,
    @SerializedName("role") val role: String
)

data class ScanRequest(
    @SerializedName("passId") val passId: String
)

data class ScanResponse(
    @SerializedName("valid") val valid: Boolean,
    @SerializedName("message") val message: String,
    @SerializedName("pass") val pass: PassResponse?
)

data class PassRequest(
    @SerializedName("reason") val reason: String? = null
)

data class PassResponse(
    @SerializedName("_id") val id: String,
    @SerializedName("studentId") val studentId: String,
    @SerializedName("qrCode") val qrCode: String?,
    @SerializedName("status") val status: String,
    @SerializedName("expiresAt") val expiresAt: String,
    @SerializedName("createdAt") val createdAt: String
)