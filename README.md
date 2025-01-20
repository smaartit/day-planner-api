# DayPlanner API

The **DayPlanner API** is a backend solution for managing tasks in a day planner. It allows users to create, update, retrieve, and delete tasks. The API integrates with AWS DynamoDB for data storage and uses .NET with Swagger for seamless development and testing.

---

## Features

- **Task Management**: Create, read, update, and delete tasks.
- **User-Specific Tasks**: Query tasks by `userId`.
- **AWS DynamoDB Integration**: Reliable and scalable data storage.

---

## Getting Started

### Prerequisites

1. **Tools**:
    - .NET 8 SDK.
    - AWS account with DynamoDB setup.
    - Postman or any HTTP client for testing.

2. **DynamoDB Table Setup**:
    - Table Name: `DayPlannerTasks`
    - Primary Key: `id` (Partition Key)
    - Sort Key: `userId`
    - Global Secondary Index (GSI): `userIdIndex`

3. **Environment Variables**:
   Set the following environment variables in your application:
    - `AWS_ACCESS_KEY_ID`
    - `AWS_SECRET_ACCESS_KEY`
    - `AWS_REGION`

---

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/DayPlannerAPI.git
   cd DayPlannerAPI

### API Endpoints
Base URL:
http://localhost:5000/api/

### Endpoints:
1. Get All Tasks
```bash
   URL: /tasks
   Method: GET
   Query Parameters:
   userId (required): The user's ID.
  ```
2. Get Task By ID

```bash
   URL: /tasks/{id}
   Method: GET
   Path Parameter:
   id (required): The task ID.
   
```

3. Create Task

```bash
   URL: /tasks
   Method: POST
   Body:
   {
   "id": 1,
   "userId": 123,
   "description": "Complete the DayPlanner API",
   "color": "#FF5733",
   "allDay": false,
   "start": "2025-01-22T09:00:00Z",
   "end": "2025-01-22T10:00:00Z"
   }
 ```

4. Update Task

```bash
   URL: /tasks/{id}
   Method: PUT
   Path Parameter:
   id (required): The task ID.
   Body: {
   "description": "Complete the DayPlanner API",
   "color": "#FF5733"
   }
  ```
5. Delete Task

```bash
   URL: /tasks/{id}
   Method: DELETE
   Path Parameter:
   id (required): The task ID.
  ```
### Error Handling
- 400 Bad Request: Invalid input data.
- 404 Not Found: Task or resource not found.
- 500 Internal Server Error: Unexpected server error.