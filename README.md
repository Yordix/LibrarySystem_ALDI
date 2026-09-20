# Library System API

This project is a simple ASP.NET Core Web API for managing books, users and loans.

## Technologies
- .NET 8
- ASP.NET Core Web API
- EFC (Entity Framework Core)
- EF Core InMemory Database 
- Swagger / OpenAPI
- xUnit for Testing

## Project Structure
Library System  
│  
│── LibrarySystem.API  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│── Controllers  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│── Data  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│── DTOs  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│── Exceptions  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│── Models  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│── Services  
│  
│── LibrarySystem.Tests

The API project contains the application logic and endpoints.  
The Test project contains unit tests for the service classes.

## How to Run the Project
***Use Visual Studio for easiest start***
1. Open Visual Studio (for example 2022)
2. Make sure .NET 8 SDK is installed on your system
3. Restore all the NuGet packages
4. Set LibrarySystem.API as the startup project.
5. Run the application with  
> **F5**   

or   

> **Ctrl+F5**  

Swagger will opens automatically in the browser.  
The database uses EF Core InMemory, no external database needed.  
When app restarted all data will be lost.

------
### If you don't want to use Visual Studio:
1. Make sure that .NET 8 SDK is installed
2. Open the terminal in the solution folder.
3. Restore the NuGet packages:
> dotnet resore  

4. Start the API project:
> dotnet run --project LibrarySystem

The terminal will show the URL where the API is running. Open the browser with that URL and add ***/swagger*** to access the swagger UI.

**For example:**
https://localhost:xxxx/swagger (for me it's 5042)

## API Endpoints
### Books

|   HTTP Request   |     Endpoint    |
| ---------------- | --------------- |
| **GET**          | /api/books      |
| **GET**          | /api/books/{id} |
| **POST**         | /api/books      |
| **PUT**          | /api/books/{id} |
| **DELETE**       | /api/books/{id} |

Books can be filtered by availability and author.

#### EXAMPLE:
> **GET** &nbsp;&nbsp;&nbsp; /api/books?available=true  
> **GET** &nbsp;&nbsp;&nbsp; /api/books?author=Orwell

### Users

|   HTTP Request   |     Endpoint    |
| ---------------- | --------------- |
| **GET**          | /api/users      |
| **POST**         | /api/users      |

### Loans

|   HTTP Request   |         Endpoint       |
| ---------------- | ---------------------- |
| **GET**          | /api/loans/active      |
| **POST**         | /api/loans             |
| **PUT**          | /api/loans/{id}/return |

## Business Logics/Rules in Bulletpoints
- A book can only be borrowed if it's available
- A book becomes unavailable if when it is being borrowed
- After return the book becomes available again
- While the book is being borrowed it cannot be deleted
- A user must be registered before a book being assigned to it (borrowing it)
- A book must be added into the collection before assigned to a user
- Duplicated ISBN numbers on books are not permitted
- Duplicated user emails are not permitted

## Architecture
The project uses a simple layered architectural structure:

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;**Controller**  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;↓  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;**Service**  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;↓  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;**DbContext**  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;↓  
**InMemory Database**

- Controllers handle HTTP Requests and responses.  
- Services contain the business logic
- ***LibraryDbContext*** is for accessing the stored data
- DTOs are used for API request and responses instead of exposing database models

## Exception Handling
The application contains basic exception handling.  

It is done with try/catch blocks. The services will throw exceptions when business rules are violated and the controllers will convert them into HTTP Responses. 

## Validation
Basic Validation is implemented within the project using Data Annotations.

- For Required fields (***[Required]***)
- Email validation (***[EmailAddress]***)
- Published Year validation (***[Range(0,2100)]***)

Invalid request models will return HTTP 400 response.

## Testing
The project contains xUnit tests for the main services:

- BookServiceTests
- UserServiceTests
- LoanServiceTests

The tests use a different EFCore InMemory database.  
Some example scenarios:
- creating books
- deleting books
- returning book
- borrowing book
- listing active loans
- **etc.**

Tests can be run within Visual Studio also:
> **Test menu item** -> **Run All Tests**

## Example Workflow

1. Registering a user with &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; **POST** &nbsp; /api/users
2. Creating a book with &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; **POST** &nbsp; /api/books
3. Borrow the book with &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; **POST** &nbsp; /api/loans
4. Check the active loans with &nbsp;&nbsp;&nbsp; **GET** &nbsp; /api/loans/active
5. Return the book with &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; **PUT** &nbsp; /api/loans/{id}/return

