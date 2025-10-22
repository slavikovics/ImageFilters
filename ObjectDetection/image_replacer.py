import cv2
import numpy as np
from ultralytics import YOLO
import os
import uuid
from typing import Tuple, List, Optional
import math

class ImageReplacer:
    def __init__(self, model_path: str):
        self.model = YOLO(model_path)
        self.class_names = [
            'plane', 'ship', 'storage tank', 'baseball diamond', 'tennis court', 
            'bridge', 'ground track field', 'large vehicle', 'small vehicle', 
            'helicopter', 'swimming pool', 'roundabout', 'soccer ball field', 
            'harbor'
        ]
    
    def calculate_rotation_angle(self, points: np.ndarray) -> float:
        if len(points) != 4:
            return 0.0
        
        points = np.array(points, dtype=np.float32)
        pts = self._order_points_clockwise(points)
        edge = pts[1] - pts[0]
        angle_rad = math.atan2(edge[1], edge[0])
        angle_deg = (math.degrees(angle_rad) + 360) % 360
        return angle_deg
    
    def rotate_image(self, image: np.ndarray, angle: float) -> np.ndarray:
        if angle == 0:
            return image
        
        height, width = image.shape[:2]
        center = (width / 2.0, height / 2.0)
        
        rotation_matrix = cv2.getRotationMatrix2D(center, angle, 1.0)
        
        abs_cos = abs(rotation_matrix[0, 0])
        abs_sin = abs(rotation_matrix[0, 1])
        new_width = int((height * abs_sin) + (width * abs_cos))
        new_height = int((height * abs_cos) + (width * abs_sin))
        
        rotation_matrix[0, 2] += (new_width / 2) - center[0]
        rotation_matrix[1, 2] += (new_height / 2) - center[1]
        
        rotated_image = cv2.warpAffine(image, rotation_matrix, (new_width, new_height), 
                                      flags=cv2.INTER_LINEAR, borderMode=cv2.BORDER_CONSTANT)
        
        return rotated_image
    
    def extract_rotated_region(self, image: np.ndarray, points: np.ndarray) -> Tuple[np.ndarray, float]:
        points = np.array(points, dtype=np.float32)
        if points.shape != (4, 2):
            return image, 0.0
        
        rect = self._order_points_clockwise(points)
        
        widthA = np.linalg.norm(rect[2] - rect[3])
        widthB = np.linalg.norm(rect[1] - rect[0])
        maxWidth = max(int(widthA), int(widthB), 1)
        
        heightA = np.linalg.norm(rect[1] - rect[2])
        heightB = np.linalg.norm(rect[0] - rect[3])
        maxHeight = max(int(heightA), int(heightB), 1)
        
        dst = np.array([
            [0, 0],
            [maxWidth - 1, 0],
            [maxWidth - 1, maxHeight - 1],
            [0, maxHeight - 1]
        ], dtype=np.float32)
        
        M = cv2.getPerspectiveTransform(rect, dst)
        warped = cv2.warpPerspective(image, M, (maxWidth, maxHeight), flags=cv2.INTER_LINEAR, borderMode=cv2.BORDER_CONSTANT)
        
        angle = self.calculate_rotation_angle(points)
        return warped, angle
    
    def replace_rotated_region(self, original_image: np.ndarray, replacement_image: np.ndarray, 
                             points: np.ndarray, angle: float) -> np.ndarray:
        result_image = original_image.copy()
        
        pts = np.array(points, dtype=np.float32)
        if pts.shape != (4, 2):
            return result_image
        
        rect = self._order_points_clockwise(pts)
        
        widthA = np.linalg.norm(rect[2] - rect[3])
        widthB = np.linalg.norm(rect[1] - rect[0])
        maxWidth = max(int(round(widthA)), int(round(widthB)), 1)
        
        heightA = np.linalg.norm(rect[1] - rect[2])
        heightB = np.linalg.norm(rect[0] - rect[3])
        maxHeight = max(int(round(heightA)), int(round(heightB)), 1)
        
        try:
            replacement_resized = cv2.resize(replacement_image, (maxWidth, maxHeight), interpolation=cv2.INTER_AREA)
        except Exception:
            replacement_resized = replacement_image.copy()
        
        src = np.array([
            [0, 0],
            [replacement_resized.shape[1] - 1, 0],
            [replacement_resized.shape[1] - 1, replacement_resized.shape[0] - 1],
            [0, replacement_resized.shape[0] - 1]
        ], dtype=np.float32)
        
        dst = rect.astype(np.float32)
        
        try:
            M = cv2.getPerspectiveTransform(src, dst)
        except Exception as e:
            print(f"Warning: perspective transform failed: {e}")
            return result_image
        
        warped = cv2.warpPerspective(replacement_resized, M, (original_image.shape[1], original_image.shape[0]),
                                     flags=cv2.INTER_LINEAR, borderMode=cv2.BORDER_CONSTANT)
        
        if warped.ndim == 3:
            mask = np.any(warped != 0, axis=2)
        else:
            mask = warped != 0
        
        if not np.any(mask):
            return result_image
        
        result_image[mask] = warped[mask]
        
        return result_image
    
    def _order_points_clockwise(self, pts: np.ndarray) -> np.ndarray:
        rect = np.zeros((4, 2), dtype=np.float32)
        center = np.mean(pts, axis=0)
        angles = np.arctan2(pts[:,1] - center[1], pts[:,0] - center[0])
        sort_idx = np.argsort(angles)
        sorted_pts = pts[sort_idx]

        top_left_idx = np.argmin(sorted_pts[:,1] + sorted_pts[:,0] * 1e-6)
        ordered = np.roll(sorted_pts, -top_left_idx, axis=0)

        v1 = ordered[1] - ordered[0]
        v2 = ordered[2] - ordered[1]
        cross = v1[0] * v2[1] - v1[1] * v2[0]
        if cross < 0:
            ordered = ordered[::-1]
            top_left_idx = np.argmin(ordered[:,1] + ordered[:,0] * 1e-6)
            ordered = np.roll(ordered, -top_left_idx, axis=0)
        return ordered.astype(np.float32)
    
    def detect_objects(self, image_path: str):
        return self.model(image_path)
    
    def replace_objects_in_image(self, original_image_path: str, replacement_image_path: str, 
                               target_class: str = "plane", confidence_threshold: float = 0.5) -> str:
        try:
            print(f"Processing: {original_image_path}")
            print(f"Replacement: {replacement_image_path}")
            print(f"Target class: {target_class}")
            
            original_img = cv2.imread(original_image_path)
            replacement_img = cv2.imread(replacement_image_path)
            
            if original_img is None:
                raise ValueError("Could not load original image")
            if replacement_img is None:
                raise ValueError("Could not load replacement image")
            
            print(f"Original image shape: {original_img.shape}")
            print(f"Replacement image shape: {replacement_img.shape}")
            
            results = self.detect_objects(original_image_path)
            detections = results[0]
            
            if hasattr(detections, 'obb') and detections.obb is not None:
                boxes = detections.obb.xyxyxyxy.cpu().numpy() if getattr(detections.obb, 'xyxyxyxy', None) is not None else np.array([])
                classes = detections.obb.cls.cpu().numpy() if getattr(detections.obb, 'cls', None) is not None else np.array([])
                confidences = detections.obb.conf.cpu().numpy() if getattr(detections.obb, 'conf', None) is not None else np.array([])
                is_obb = True
            else:
                boxes = detections.boxes.xyxy.cpu().numpy() if getattr(detections.boxes, 'xyxy', None) is not None else np.array([])
                classes = detections.boxes.cls.cpu().numpy() if getattr(detections.boxes, 'cls', None) is not None else np.array([])
                confidences = detections.boxes.conf.cpu().numpy() if getattr(detections.boxes, 'conf', None) is not None else np.array([])
                is_obb = False
            
            result_img = original_img.copy()
            replacements_made = 0
            
            if boxes is None:
                boxes = np.array([])
            
            print(f"Found {len(boxes)} detections")
            print(f"OBB mode: {is_obb}")
            
            for i, box in enumerate(boxes):
                if i >= len(classes):
                    continue
                    
                class_id = int(classes[i])
                confidence = float(confidences[i]) if i < len(confidences) else 0.0
                
                if class_id >= len(self.class_names):
                    continue
                    
                class_name = self.class_names[class_id]
                
                if class_name == target_class and confidence > confidence_threshold:
                    print(f"Replacing {target_class} with confidence {confidence:.2f}")
                    
                    if is_obb and box.size == 8:
                        points = box.reshape(4, 2)
                        result_img = self.replace_rotated_region(
                            result_img, replacement_img, points, 
                            self.calculate_rotation_angle(points)
                        )
                    else:
                        x1, y1, x2, y2 = np.array(box, dtype=int)
                        
                        x1 = max(0, min(x1, result_img.shape[1] - 1))
                        y1 = max(0, min(y1, result_img.shape[0] - 1))
                        x2 = max(0, min(x2, result_img.shape[1] - 1))
                        y2 = max(0, min(y2, result_img.shape[0] - 1))
                        
                        width = x2 - x1
                        height = y2 - y1
                        
                        if width >= 10 and height >= 10:
                            try:
                                resized_replacement = cv2.resize(replacement_img, (width, height), interpolation=cv2.INTER_AREA)
                                result_img[y1:y2, x1:x2] = resized_replacement
                            except Exception as e:
                                print(f"Failed to replace at ({x1},{y1})-({x2},{y2}): {e}")
                                continue
                    
                    replacements_made += 1
            
            print(f"Successfully made {replacements_made} replacements")
            
            output_filename = f"replaced_{uuid.uuid4().hex[:8]}.jpg"
            output_path = os.path.join("detection_results", output_filename)
            os.makedirs("detection_results", exist_ok=True)
            
            success = cv2.imwrite(output_path, result_img)
            if not success:
                raise Exception("Failed to write image file")
                
            return output_path
            
        except Exception as e:
            print(f"Error in replace_objects_in_image: {str(e)}")
            import traceback
            traceback.print_exc()
            raise
