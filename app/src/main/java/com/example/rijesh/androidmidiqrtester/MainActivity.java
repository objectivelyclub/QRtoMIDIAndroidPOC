package com.example.rijesh.androidmidiqrtester;

import org.apache.commons.codec.binary.Base64;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import com.google.zxing.Result;
import me.dm7.barcodescanner.zxing.ZXingScannerView;

public class MainActivity extends AppCompatActivity implements ZXingScannerView.ResultHandler  {
    private ZXingScannerView mScannerView;
    private MidiDevice midiDev;
    private long timeSinceLastProcess = System.currentTimeMillis();
    private byte current = 27;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        midiDev = new MidiDevice();
        startScanner();
        //setContentView(R.layout.activity_main);

    }

    public void PlayTone(View v){
        midiDev.playCNote();
    }

    public void QrScanner(View view){
        startScanner();
    }

    public void startScanner(){
        mScannerView = new ZXingScannerView(this);   // Programmatically initialize the scanner view
        setContentView(mScannerView);

        mScannerView.setResultHandler(this); // Register ourselves as a handler for scan results.
        mScannerView.startCamera();         // Start camera
    }

    @Override
    public void handleResult(Result rawResult) {
        /*long tmp = System.currentTimeMillis();
        if ((tmp - timeSinceLastProcess) > 2000){
            timeSinceLastProcess = System.currentTimeMillis();
            midiDev.addToQueue(Base64.decodeBase64(rawResult.toString().getBytes())); // Decodes QRcode into byte array.
        }
        */
        byte[] b = Base64.decodeBase64(rawResult.toString().getBytes());

        if (b[0] != current) {
            current = b[0];
            midiDev.addToQueue(b);
        }
        mScannerView.resumeCameraPreview(this);
    }

    @Override
    protected void onResume()
    {
        super.onResume();
    }

    @Override
    public void onPause() {
        super.onPause();
        midiDev.pause();
        mScannerView.stopCamera();           // Stop camera on pause
    }
}
