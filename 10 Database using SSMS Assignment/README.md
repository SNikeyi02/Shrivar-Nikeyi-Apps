Please use link to access file: [INSY6112 Exam.pdf](https://github.com/user-attachments/files/30760324/INSY6112.Exam.pdf)

# INSY6112 Exam Submission

**Student Name:** Shrivar Nikeyi  
**Student ID:** ST10435835

---

## Question 1: Entity Relationship Diagram (ERD)

*The ERD model structure is provided below and is also attached as a separate Visio document.*

```text
       +--------------------+
       |       Recipe       |
       +--------------------+
       | PK | Recipe_ID     |
       |    | Recipe_Name   |
       |    | Recipe_Type   |
       +---------+----------+
                 |
                 | 1
                 |
                 | Can Have
                 |
                 | M
                 v
+-----------------------------------+
|         Recipe_Ingredient         |
+-----------------------------------+
| PK | Recipe_Ingredient_ID        |
|    | Recipe_Ingredient_Name       |
| FK | Recipe_ID                    |
| FK | Ingredient_ID                |
+-----------------------------------+
                 ^
                 | M
                 |
                 | Requires
                 |
                 | 1
       +---------+----------+
       |     Ingredient     |
       +--------------------+
       | PK | Ingredient_ID |
       |    | Ingredient_Name|
       |    | Ingredient_Type|
       | FK | Category_ID   |
       +---------+----------+
                 |
                 | M
                 |
                 | Belongs To
                 |
                 | 1
                 v
       +--------------------+
       |      Category      |
       +--------------------+
       | PK | Category_ID   |
       |    | Category_Name |
       |    | Category_Description |
       +---------+----------+
                 ^
                 | 1
                 |
                 | Belongs To
                 |
                 | M
       +---------+----------+
       |   Recipe_Category  |
       +--------------------+
       | PK | Recipe_Category_ID |
       |    | Recipe_Category_Name|
       | FK | Category_ID   |
       | FK | Recipe_ID     |
       +--------------------+




