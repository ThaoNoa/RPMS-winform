# Biểu đồ Use Case – Hệ thống RPMS

## File đầu ra (chèn báo cáo đồ án)

| Hình | Mô tả | File |
|------|--------|------|
| **2.1** | Use Case tổng thể (Actors, packages, association, include, extend, generalization) | `Hinh_2_1_UseCase_TongThe_RPMS.png` / `.svg` |
| **2.1a** | Chi tiết quan hệ `<<include>>` và `<<extend>>` | `Hinh_2_1a_UseCase_QuanHe.png` / `.svg` |

Nguồn chỉnh sửa (PlantUML):

- `Hinh_2_1_UseCase_TongThe_RPMS.puml`
- `Hinh_2_1a_UseCase_QuanHe.puml`
- `Hinh_2_1_UseCase_DayDu.puml` (bản rút gọn nhãn, cùng nội dung với 2.1)

## Nội dung UML

- **Actors:** User (cơ sở), Admin, Landlord, Tenant, Manager, Guest  
- **Generalization:** Admin / Landlord / Tenant / Manager `--|>` User  
- **System boundary:** *Hệ thống RPMS (Rental Property Management System)*  
- **Packages:** a→i theo nhóm chức năng (màu phân biệt)  
- **`<<include>>`:** Login, Check Permission, Send Notification, Log Activity  
- **`<<extend>>`:** Pay Invoice, Accept Contract, Book Appointment, Add Favorite, Reply Review, Assign Tenant  

## Render lại (PNG / SVG)

Yêu cầu: Java 17+ và `tools/plantuml.jar`.

```powershell
cd Docs\FlowDiagrams
java "-DPLANTUML_LIMIT_SIZE=24576" -jar tools\plantuml.jar -tpng -tsvg -charset UTF-8 `
  Hinh_2_1_UseCase_TongThe_RPMS.puml `
  Hinh_2_1a_UseCase_QuanHe.puml
```

Khuyến nghị: chèn **SVG** vào Word/LaTeX (vector, phóng to không vỡ), hoặc PNG khổ **A3 ngang**.

## Ghi chú

- Quan hệ `<<include>> Login` trên Hình 2.1 được vẽ **đại diện theo nhóm** để tránh rối mũi tên; khi triển khai mọi UC (trừ Register / Guest xem tin) đều yêu cầu đăng nhập.
- File Mermaid cũ `Hinh_2_1_UseCase_TongThe_RPMS.mmd` chỉ là bản phác thảo; nguồn chuẩn là PlantUML.
