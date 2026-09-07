package com.example.campuspass.ui.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.campuspass.data.api.RetrofitClient
import com.example.campuspass.data.model.PassRequest
import com.example.campuspass.data.model.PassResponse
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

sealed interface PassUiState {
    object Idle : PassUiState
    object Loading : PassUiState
    data class Success(val passes: List<PassResponse>) : PassUiState
    data class Error(val message: String) : PassUiState
}

class PassViewModel : ViewModel() {

    private val _uiState = MutableStateFlow<PassUiState>(PassUiState.Idle)
    val uiState: StateFlow<PassUiState> = _uiState.asStateFlow()

    fun fetchMyPasses() {
        viewModelScope.launch {
            _uiState.value = PassUiState.Loading
            try {
                val response = RetrofitClient.apiService.getMyPasses()
                if (response.isSuccessful && response.body() != null) {
                    _uiState.value = PassUiState.Success(response.body()!!)
                } else {
                    _uiState.value = PassUiState.Error("Server error: ${response.code()}")
                }
            } catch (e: Exception) {
                _uiState.value = PassUiState.Error(e.localizedMessage ?: "Network request failed")
            }
        }
    }

    fun generatePass(reason: String? = null) {
        viewModelScope.launch {
            _uiState.value = PassUiState.Loading
            try {
                val response = RetrofitClient.apiService.generatePass(PassRequest(reason))
                if (response.isSuccessful) {
                    fetchMyPasses()
                } else {
                    _uiState.value = PassUiState.Error("Failed to generate pass: ${response.code()}")
                }
            } catch (e: Exception) {
                _uiState.value = PassUiState.Error(e.localizedMessage ?: "Network request failed")
            }
        }
    }
}