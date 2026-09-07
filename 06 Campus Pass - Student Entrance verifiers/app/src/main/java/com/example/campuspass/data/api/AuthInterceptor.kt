package com.example.campuspass.data.api

import com.google.android.gms.tasks.Tasks
import com.google.firebase.auth.FirebaseAuth
import okhttp3.Interceptor
import okhttp3.Response

class AuthInterceptor : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val originalRequest = chain.request()
        val user = FirebaseAuth.getInstance().currentUser

        val token = if (user != null) {
            try {
                val tokenResult = Tasks.await(user.getIdToken(false))
                tokenResult.token
            } catch (e: Exception) {
                null
            }
        } else null

        val requestBuilder = originalRequest.newBuilder()
        if (!token.isNullOrEmpty()) {
            requestBuilder.addHeader("Authorization", "Bearer $token")
        }

        return chain.proceed(requestBuilder.build())
    }
}