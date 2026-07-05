# Testing Data Validation

## Overview
קובץ זה כולל דוגמאות בדיקה ל־Data Annotations (ולידציה) בפרויקט ASP.NET Core.
המטרה היא להדגים כיצד השרת מבצע ולידציה אוטומטית על DTOs ומחזיר תגובות שגיאה מסודרות במקרה של נתונים לא תקינים.

כל בקשה נבדקת בצד השרת לפי החוקים שהוגדרו ב־Data Annotations, ובמקרה של שגיאה מוחזר 400 Bad Request עם פירוט השדות השגויים.

הדוגמאות בקובץ נועדו לעזור למפתחים לבדוק את ה־API בצורה מהירה ולהבין את כללי הוולידציה במערכת.


## GiftDTO
✔ valid
❌ invalid (name empty)

## UserDTO
✔ valid
❌ invalid email

## OrderDTO
✔ valid
❌ invalid totalAmount

## Notes
- validation אוטומטי
- 400 Bad Request
- errors object