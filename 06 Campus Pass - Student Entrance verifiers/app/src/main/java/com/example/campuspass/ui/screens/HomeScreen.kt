package com.example.campuspass.ui.screens

import android.graphics.Bitmap
import android.graphics.Color
import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ExitToApp
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.asImageBitmap
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.example.campuspass.data.model.PassResponse
import com.example.campuspass.ui.viewmodel.PassUiState
import com.example.campuspass.ui.viewmodel.PassViewModel
import com.google.firebase.auth.FirebaseAuth
import com.google.zxing.BarcodeFormat
import com.google.zxing.qrcode.QRCodeWriter

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun HomeScreen(
    onSignOut: () -> Unit,
    viewModel: PassViewModel = viewModel()
) {
    val uiState by viewModel.uiState.collectAsState()

    LaunchedEffect(Unit) {
        viewModel.fetchMyPasses()
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Campus Pass Dashboard") },
                actions = {
                    IconButton(onClick = {
                        FirebaseAuth.getInstance().signOut()
                        onSignOut()
                    }) {
                        Icon(
                            imageVector = Icons.AutoMirrored.Filled.ExitToApp,
                            contentDescription = "Sign Out"
                        )
                    }
                }
            )
        },
        floatingActionButton = {
            ExtendedFloatingActionButton(
                onClick = { viewModel.generatePass("Campus Access") }
            ) {
                Text("Request Pass")
            }
        }
    ) { paddingValues ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .padding(16.dp),
            contentAlignment = Alignment.Center
        ) {
            when (val state = uiState) {
                is PassUiState.Loading -> CircularProgressIndicator()
                is PassUiState.Error -> Text("Error: ${state.message}", color = MaterialTheme.colorScheme.error)
                is PassUiState.Success -> {
                    if (state.passes.isEmpty()) {
                        Text("No active passes found. Tap 'Request Pass' to generate one.")
                    } else {
                        LazyColumn(
                            modifier = Modifier.fillMaxSize(),
                            verticalArrangement = Arrangement.spacedBy(16.dp)
                        ) {
                            items(state.passes) { pass ->
                                PassCard(pass = pass)
                            }
                        }
                    }
                }
                PassUiState.Idle -> Text("Initializing...")
            }
        }
    }
}

@Composable
fun PassCard(pass: PassResponse) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        elevation = CardDefaults.cardElevation(defaultElevation = 4.dp)
    ) {
        Column(
            modifier = Modifier.fillMaxWidth().padding(16.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Text(text = "Pass ID: ${pass.id}", style = MaterialTheme.typography.titleMedium)
            Spacer(modifier = Modifier.height(4.dp))
            Text(text = "Status: ${pass.status}", style = MaterialTheme.typography.bodyMedium)
            Text(text = "Expires: ${pass.expiresAt}", style = MaterialTheme.typography.bodySmall)

            val qrBitmap = remember(pass.id) { generateQrCode(pass.id) }

            qrBitmap?.let { bitmap ->
                Spacer(modifier = Modifier.height(16.dp))
                Image(
                    bitmap = bitmap.asImageBitmap(),
                    contentDescription = "Pass QR Code",
                    modifier = Modifier.size(180.dp)
                )
            }
        }
    }
}

fun generateQrCode(content: String): Bitmap? {
    return try {
        val bitMatrix = QRCodeWriter().encode(content, BarcodeFormat.QR_CODE, 512, 512)
        val width = bitMatrix.width
        val height = bitMatrix.height
        val bitmap = Bitmap.createBitmap(width, height, Bitmap.Config.RGB_565)
        for (x in 0 until width) {
            for (y in 0 until height) {
                bitmap.setPixel(x, y, if (bitMatrix[x, y]) Color.BLACK else Color.WHITE)
            }
        }
        bitmap
    } catch (e: Exception) {
        null
    }
}