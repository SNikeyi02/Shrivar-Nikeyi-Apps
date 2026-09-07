package com.example.campuspass.data.repository

import android.content.Context
import androidx.credentials.CredentialManager
import androidx.credentials.GetCredentialRequest
import androidx.credentials.CustomCredential
import com.example.campuspass.data.api.CampusPassApi
import com.example.campuspass.data.model.UserDto
import com.google.android.libraries.identity.googleid.GetGoogleIdOption
import com.google.android.libraries.identity.googleid.GoogleIdTokenCredential
import com.google.firebase.auth.FirebaseAuth
import com.google.firebase.auth.GoogleAuthProvider
import kotlinx.coroutines.tasks.await

class AuthRepository(
    private val api: CampusPassApi,
    private val firebaseAuth: FirebaseAuth = FirebaseAuth.getInstance()
) {
    suspend fun signInWithGoogle(context: Context, webClientId: String): Result<UserDto> {
        return runCatching {
            val credentialManager = CredentialManager.create(context)

            val googleIdOption = GetGoogleIdOption.Builder()
                .setFilterByAuthorizedAccounts(false)
                .setServerClientId(webClientId)
                .build()

            val request = GetCredentialRequest.Builder()
                .addCredentialOption(googleIdOption)
                .build()

            val result = credentialManager.getCredential(context, request)
            val credential = result.credential

            if (credential !is CustomCredential || credential.type != GoogleIdTokenCredential.TYPE_GOOGLE_ID_TOKEN_CREDENTIAL) {
                throw IllegalStateException("Invalid credential received.")
            }

            val googleIdTokenCredential = GoogleIdTokenCredential.createFrom(credential.data)
            val firebaseCredential = GoogleAuthProvider.getCredential(googleIdTokenCredential.idToken, null)

            firebaseAuth.signInWithCredential(firebaseCredential).await()

            val response = api.syncUser()
            if (response.isSuccessful && response.body() != null) {
                response.body()!!
            } else {
                val errorMsg = response.errorBody()?.string() ?: "Sync failed"
                throw Exception("HTTP ${response.code()}: $errorMsg")
            }
        }
    }

    fun signOut() {
        firebaseAuth.signOut()
    }
}