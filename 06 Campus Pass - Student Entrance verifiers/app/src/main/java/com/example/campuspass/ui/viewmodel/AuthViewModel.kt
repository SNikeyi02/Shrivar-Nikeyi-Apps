package com.example.campuspass.ui.viewmodel

import android.content.Context
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.campuspass.data.model.UserDto
import com.example.campuspass.data.repository.AuthRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

sealed interface AuthUiState {
    object Idle : AuthUiState
    object Loading : AuthUiState
    data class Success(val user: UserDto) : AuthUiState
    data class Error(val message: String) : AuthUiState
}

class AuthViewModel(
    private val repository: AuthRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow<AuthUiState>(AuthUiState.Idle)
    val uiState: StateFlow<AuthUiState> = _uiState.asStateFlow()

    fun handleGoogleSignIn(context: Context, webClientId: String) {
        viewModelScope.launch {
            _uiState.value = AuthUiState.Loading
            repository.signInWithGoogle(context, webClientId)
                .onSuccess { user -> _uiState.value = AuthUiState.Success(user) }
                .onFailure { error -> _uiState.value = AuthUiState.Error(error.localizedMessage ?: "Sign in failed") }
        }
    }

    fun signOut() {
        repository.signOut()
        _uiState.value = AuthUiState.Idle
    }
}