from fastapi import FastAPI, File, UploadFile, HTTPException
from fastapi.responses import FileResponse
from image_replacer import ImageReplacer
import cv2
import numpy as np
from ultralytics import YOLO
import os
import uuid
from pathlib import Path
import shutil

app = FastAPI(title="Aerial Object Detection API")

model = None
MODEL_PATH = "../runs/obb/train/weights/best.pt"
replacer = ImageReplacer("../runs/obb/train/weights/best.pt")
OUTPUT_DIR = "detection_results"
os.makedirs(OUTPUT_DIR, exist_ok=True)

CLASS_NAMES = [
    'plane', 'ship', 'storage tank', 'baseball diamond', 'tennis court', 
    'basketball court', 'ground track field', 'harbor', 'brigde', 'large vehicle', 'small vehicle', 
    'helicopter', 'roundabout', 'soccer ball field', 'swimming pool',
]

@app.on_event("startup")
async def load_model():
    global model
    try:
        if os.path.exists(MODEL_PATH):
            model = YOLO(MODEL_PATH)
            print(f"✅ Model loaded successfully from {MODEL_PATH}")
        else:
            print(f"⚠️  Model not found at {MODEL_PATH}, using pretrained as fallback")
            model = YOLO("yolo11n-obb.pt")
    except Exception as e:
        print(f"❌ Error loading model: {e}")
        model = YOLO("yolo11n-obb.pt")

def save_upload_file(upload_file: UploadFile, save_path: str):
    with open(save_path, "wb") as buffer:
        shutil.copyfileobj(upload_file.file, buffer)

def process_image(image_path: str, output_suffix: str = "detected"):
    if model is None:
        raise HTTPException(status_code=500, detail="Model not loaded")
    
    original_path = Path(image_path)
    output_filename = f"{original_path.stem}_{output_suffix}_{uuid.uuid4().hex[:8]}{original_path.suffix}"
    output_path = os.path.join(OUTPUT_DIR, output_filename)
    
    results = model(image_path)
    
    results[0].save(output_path)
    
    return output_path

@app.post("/detect/")
async def detect_objects(image: UploadFile = File(...)):
    try:
        temp_path = os.path.join(OUTPUT_DIR, f"temp_{uuid.uuid4().hex[:8]}_{image.filename}")
        save_upload_file(image, temp_path)
        
        result_path = process_image(temp_path)
        os.remove(temp_path)
        
        return {
            "status": "success",
            "detected_image_path": result_path,
            "message": "Detection completed successfully"
        }
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error processing image: {str(e)}")

@app.post("/detect_and_replace/")
async def detect_and_replace_vehicles(
    original_image: UploadFile = File(...),
    replacement_image: UploadFile = File(...),
    vehicle_class: str = "plane"
):
    try:
        temp_original = os.path.join(OUTPUT_DIR, f"temp_orig_{uuid.uuid4().hex[:8]}_{original_image.filename}")
        temp_replacement = os.path.join(OUTPUT_DIR, f"temp_repl_{uuid.uuid4().hex[:8]}_{replacement_image.filename}")
        
        save_upload_file(original_image, temp_original)
        save_upload_file(replacement_image, temp_replacement)
        
        result_path = replacer.replace_objects_in_image(temp_original, temp_replacement, vehicle_class)
        
        os.remove(temp_original)
        os.remove(temp_replacement)
        
        return FileResponse(
            result_path, 
            media_type='image/jpeg',
            filename=f"detected_replaced_{vehicle_class}.jpg"
        )
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error processing images: {str(e)}")

@app.get("/results/{filename}")
async def get_result_image(filename: str):
    file_path = os.path.join(OUTPUT_DIR, filename)
    if os.path.exists(file_path):
        return FileResponse(file_path)
    else:
        raise HTTPException(status_code=404, detail="Image not found")

@app.get("/")
async def root():
    return {
        "message": "Aerial Object Detection API", 
        "endpoints": {
            "/detect/": "POST - Detect objects in image",
            "/detect_and_replace/": "POST - Detect and replace vehicles",
            "/results/{filename}": "GET - Retrieve processed image"
        }
    }

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=8000)