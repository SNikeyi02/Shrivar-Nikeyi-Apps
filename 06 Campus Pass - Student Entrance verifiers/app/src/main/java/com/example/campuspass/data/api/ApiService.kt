package com.example.campuspass.data.api

import com.example.campuspass.data.model.*
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST

interface ApiService {
    @POST("api/users/register")
    suspend fun registerUser(@Body request: RegisterRequest): Response<UserResponse>

    @GET("api/users/me")
    suspend fun getMyProfile(): Response<UserResponse>

    @POST("api/passes/generate")
    suspend fun generatePass(@Body request: PassRequest = PassRequest()): Response<PassResponse>

    @GET("api/passes/my-passes")
    suspend fun getMyPasses(): Response<List<PassResponse>>

    @POST("api/passes/scan")
    suspend fun scanPass(@Body request: ScanRequest): Response<ScanResponse>
}