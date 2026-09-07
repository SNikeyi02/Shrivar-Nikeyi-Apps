package com.example.campuspass.data.api

import com.example.campuspass.data.model.UserDto
import retrofit2.Response
import retrofit2.http.POST

interface CampusPassApi {
    @POST("users/sync")
    suspend fun syncUser(): Response<UserDto>
}