package com.example.campuspass.data.model

import com.google.gson.annotations.SerializedName

data class UserDto(
    @SerializedName("_id") val id: String,
    @SerializedName("firebaseUid") val firebaseUid: String,
    @SerializedName("email") val email: String,
    @SerializedName("displayName") val displayName: String,
    @SerializedName("role") val role: String,
    @SerializedName("createdAt") val createdAt: String,
    @SerializedName("updatedAt") val updatedAt: String
)