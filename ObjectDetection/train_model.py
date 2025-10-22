from ultralytics import YOLO
import torch

def main():
    print(f"PyTorch version: {torch.__version__}")
    print(f"CUDA available: {torch.cuda.is_available()}")
    
    if torch.cuda.is_available():
        print(f"GPU: {torch.cuda.get_device_name(0)}")
    
    model = YOLO("yolo11n-obb.pt")
    
    results = model.train(
        data="DOTAv1.yaml",
        epochs=100,
        imgsz=1024,
        batch=4,
        workers=8,
        device=0,
        cache=False,
        amp=True,
        close_mosaic=0
    )

if __name__ == '__main__':
    main()