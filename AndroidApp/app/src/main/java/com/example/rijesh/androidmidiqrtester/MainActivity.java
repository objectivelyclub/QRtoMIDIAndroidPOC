package com.example.rijesh.androidmidiqrtester;

import org.apache.commons.codec.binary.Base64;

import android.annotation.TargetApi;
import android.os.Build;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.view.WindowManager;
import FastBarcodeScanner;


import com.google.zxing.Result;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.UnsupportedEncodingException;

import me.dm7.barcodescanner.zxing.ZXingScannerView;

public class MainActivity extends AppCompatActivity implements ZXingScannerView.ResultHandler  {
    private ZXingScannerView mScannerView;
    private MidiDevice midiDev;
    private Byte current = 27;
    FasBarcodeScanner mScanner = new FastBarcodeScanner(this, null);
    mScanner.setBarcodeListener(this);
    mScanner.StartFocus();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);

        midiDev = new MidiDevice();
        startScanner();
        setContentView(R.layout.activity_main);
    }

    public void PlayTone(View v){
        midiDev.playCNote();
        String text = null;
        try {
            text = new String(readFile("/storage/AE2D-8B85/out.pmid"), "ASCII");
        } catch (UnsupportedEncodingException e) {
            e.printStackTrace();
        }
        midiDev.addToQueue(Base64.decodeBase64(text.getBytes()));
    }

    public byte[] readFile(String filepath) {
        File file = new File(filepath);
        FileInputStream fin = null;
        byte[] fileContent = null;
        try {
            // create FileInputStream object
            fin = new FileInputStream(file);

            fileContent = new byte[(int)file.length()];

            // Reads up to certain bytes of data from this input stream into an array of bytes.
            fin.read(fileContent);
            //create string from byte array
            String s = new String(fileContent);
            System.out.println("File content: " + s);
        } catch (FileNotFoundException e) {
            System.out.println("File not found" + e);
            e.printStackTrace();
        } catch (IOException ioe) {
            System.out.println("Exception while reading file " + ioe);
        } finally {
            // close the streams using close method
            try {
                if (fin != null) {
                    fin.close();
                }
            }
            catch (IOException ioe) {
                System.out.println("Error while closing stream: " + ioe);
            }
        }

        return fileContent;
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

    @TargetApi(Build.VERSION_CODES.JELLY_BEAN)
    @Override
    public void handleResult(Result rawResult) {
        mScannerView.stopCameraPreview();
        mScannerView.setAutoFocus(false);
        mScannerView.resumeCameraPreview(this);
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
