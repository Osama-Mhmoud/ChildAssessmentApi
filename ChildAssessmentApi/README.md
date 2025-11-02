# Child Assessment API - VBMAPP

نظام تقييم الأطفال باستخدام معايير VBMAPP (Verbal Behavior Milestones Assessment and Placement Program).

## الميزات

- **6 أقسام للتقييم**: التفاعل الاجتماعي، التواصل، السلوك، المعالجة الحسية، المهارات الحركية، التطور الإدراكي
- **نظام VBMAPP**: دعم مستويات التطور من 0-48 شهر
- **استيراد الأسئلة**: من ملفات JSON أو Excel
- **نظام التقييم**: تسجيل إجابات الأطفال وتقييمهم
- **تقارير مفصلة**: نتائج التقييم حسب الأقسام

## هيكل المشروع

```
ChildAssessmentApi/
├── Controllers/
│   └── AssessmentsController.cs    # API endpoints
├── Models/
│   ├── Models.cs                   # نماذج البيانات
│   └── AssessmentDbContext.cs      # قاعدة البيانات
├── Services/
│   └── QuestionImportService.cs    # خدمة استيراد الأسئلة
├── Data/
│   └── vbmapp_questions.json       # ملف الأسئلة النموذجية
├── Migrations/                      # تحديثات قاعدة البيانات
└── Program.cs                      # إعدادات التطبيق
```

## ملفات الأسئلة

### موقع ملف الأسئلة
الأسئلة موجودة في مجلد `Data/vbmapp_questions.json` وتحتوي على:

- **19 سؤال نموذجي** من VBMAPP
- **6 أقسام** مختلفة للتقييم
- **مستويات تطور** من 0-3 سنوات
- **فئات مختلفة** مثل الانتباه، التفاعل الاجتماعي، إلخ

### تنسيق ملف الأسئلة

```json
{
  "questions": [
    {
      "text": "نص السؤال",
      "description": "وصف السؤال (اختياري)",
      "sectionId": 1,
      "milestoneLevel": 0,
      "category": "فئة السؤال",
      "order": 1
    }
  ]
}
```

## API Endpoints

### الأقسام والأسئلة
- `GET /api/assessments/sections` - الحصول على جميع الأقسام مع أسئلتها
- `GET /api/assessments/questions` - الحصول على جميع الأسئلة
- `GET /api/assessments/questions?sectionId=1` - أسئلة قسم معين
- `GET /api/assessments/questions/categories` - فئات الأسئلة
- `GET /api/assessments/questions/milestone-levels` - مستويات التطور

### إدارة الأسئلة
- `POST /api/assessments/questions` - إضافة سؤال جديد
- `PUT /api/assessments/questions/{id}` - تحديث سؤال
- `DELETE /api/assessments/questions/{id}` - حذف سؤال (تعطيل)
- `POST /api/assessments/questions/bulk-import` - استيراد أسئلة متعددة

### استيراد الأسئلة
- `POST /api/assessments/questions/import-from-file` - استيراد من ملف
- `POST /api/assessments/questions/import-sample` - استيراد الأسئلة النموذجية

### الأطفال والتقييمات
- `POST /api/assessments/children` - إضافة طفل جديد
- `GET /api/assessments/children/{id}` - بيانات طفل
- `POST /api/assessments/assessments` - إنشاء تقييم
- `GET /api/assessments/assessments/{id}` - بيانات التقييم
- `GET /api/assessments/assessments/{id}/result` - نتائج التقييم

## كيفية الاستخدام

### 1. تشغيل المشروع
```bash
dotnet run
```

### 2. استيراد الأسئلة النموذجية
```bash
curl -X POST "http://localhost:5016/api/assessments/questions/import-sample"
```

### 3. إضافة طفل جديد
```bash
curl -X POST "http://localhost:5016/api/assessments/children" \
  -H "Content-Type: application/json" \
  -d '{"name": "أحمد محمد", "dateOfBirth": "2020-01-15T00:00:00Z"}'
```

### 4. إنشاء تقييم
```bash
curl -X POST "http://localhost:5016/api/assessments/assessments" \
  -H "Content-Type: application/json" \
  -d '{
    "childId": 1,
    "date": "2024-01-15T00:00:00Z",
    "answers": [
      {"questionId": 1, "score": 1.0},
      {"questionId": 2, "score": 0.5}
    ]
  }'
```

## قاعدة البيانات

### الجداول
- **Sections**: الأقسام (6 أقسام)
- **Questions**: الأسئلة مع معلومات VBMAPP
- **Children**: بيانات الأطفال
- **Assessments**: التقييمات
- **Answers**: إجابات الأسئلة

### الحقول الجديدة
- `Description`: وصف السؤال
- `MilestoneLevel`: مستوى التطور (0-48 شهر)
- `Category`: فئة السؤال
- `Order`: ترتيب السؤال في القسم
- `IsActive`: حالة السؤال (نشط/معطل)

## التطوير

### إضافة أسئلة جديدة
1. أضف الأسئلة إلى `Data/vbmapp_questions.json`
2. استخدم endpoint الاستيراد لتحميلها

### إضافة أقسام جديدة
1. حدث `AssessmentDbContext.cs`
2. أضف migration جديد
3. حدث الـ controller

## المتطلبات
- .NET 9.0
- SQL Server LocalDB
- Entity Framework Core

## الرخصة
هذا المشروع مخصص للاستخدام التعليمي والبحثي.
