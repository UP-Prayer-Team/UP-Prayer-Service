API Requests
------------

Requests that require specific roles are authorized using JWT bearer authentication.  
This token is obtained using the `/api/users/authenticate` endpoint.

API Responses
-------------

Responses will be JSON objects with a boolean `success` value, and if `success` is false, a human-readable `message` string, and otherwise a `data` property.

To detect a particular reason for failure, check the HTTP status code instead of comparing against the `message` string.

Example failure response:
```json
{
    "success": false,
    "message": "User not found."
}
```

Example success response:
```json
{
    "success": true,
    "data": {
        ...
    }
}
```

In addition to each endpoints' listed possible responses, endpoints will return a `200 OK` response if the request was successfully carried out, and endpoints that specify a **Required Role** will return a `401 Unauthorized` response if the request did not contain a valid JWT that specifies a user with the required role.

Users
-----

Users are full accounts who have roles, which give them permission to do different things with the project.

Important properties of Users:
 - Unique ID
 - Username (for signin)
 - Password Hash (for signin)
 - Roles (for authorization)

Roles:
 - `admin`: Authorization to make arbitrary changes to any data (except remove their own `admin` role, for foolproofing)
 - `spectator`: Authorization to view any data (except User password hashes), and change any of their own User data (except change their own roles)

Users who have the `admin` role should also have the `spectator` role.

**`/api/users/authenticate`**
-----------------------------
**HTTP Method**: `POST`  
**Required Role**: none  
**Request Body**:
```json
{
    "username": "foo",
    "password": "password"
}
```
**Response Body**:
```json
"data": {
    "token": "..."
}
```
**Response Codes**:
 - `404 Not Found` if the given `username` and `password` combination does not match any existing Users.

**`/api/users/list`**
---------------------
**HTTP Method**: `GET`  
**Required Role**: `spectator`  
**Response Body**:
```json
"data": [
    {
        "id": "...",
        "username": "test",
        "roles": [
            "spectator",
            "admin"
        ]
    },
    ...
]
```

**`/api/users/user/<userid>`**
------------------------------
**HTTP Method**: `GET`  
**Required Role**: `spectator`  
**Response Body**:
```json
"data": {
    "id": "...",
    "username": "test",
    "roles": [
        "spectator",
        "admin"
    ]
}
```

**`/api/users/create`**
-----------------------
**HTTP Method**: `POST`  
**Required Role**: `admin`  
**Request Body**:
```json
{
    "username": "user",
    "password": "password",
    "roles": [
        "role1",
        "role2"
    ]
}
```
**Response Body**:
```json
"data": null
```
**Response Codes**:
 - `400 Bad Request` if the given `username` does not meet the username requirements, or if the given `password` does not meet the password requirements, or if any of the `role` entries is not a valid role name, or if a user with the given `username` already exists.

**`/api/users/update`**
-----------------------
**HTTP Method**: `POST`  
**Required Role**: `spectator`  
**Request Body**:
```json
{
    "id": "...",
    "username": "name",
    "roles": [
        "role1",
        "role2"
    ]
}
```
**Response Body**:
```json
"data": null
```
**Response Codes**:
 - `404 Not Found` if no User with the given `id` exists.
 - `401 Unauthorized` if the current User does not have the `admin` role and is either trying to edit their roles or trying to edit a user other than themselves, or if the current User is trying to remove their own `admin` role.
 - `400 Bad Request` if the given `username` does not meet the username requirements, or if any of the `role` entries is not a valid role name, or if a different user already exists with the given `username`.

**`/api/users/resetpassword`**
------------------------------
**HTTP Method**: `POST`  
**Required Role**: `admin`  
**Request Body**:
```json
{
    "id": "..."
}
```
**Response Body**:
```json
{
    "password": "SoMeGeNeRaTeDpAsSwOrD!!"
}
```
**Response Codes**:
 - `404 Not Found` if no User with the given `id` exists.

**`/api/users/setpassword`**
----------------------------
**HTTP Method**: `POST`  
**Required Role**: `spectator`  
**Request Body**:
```json
{
    "password": "new password"
}
```
**Response Body**:
```json
"data": null
```
**Response Codes**:
 - `400 Bad Request` if the given `password` does not meet the password requirements.

**`/api/users/delete`**
-----------------------
**HTTP Method**: `POST`  
**Required Role**: `admin`  
**Request Body**:
```json
{
    "id": "..."
}
```
**Response Body**:
```json
"data": null
```
**Response Codes**:
 - `404 Not Found` if no User with the given `id` exists.
 - `403 Forbidden` if the current User is trying to delete themselves.

Reservations
------------

Reservations are a claim by a single project visitor for a single time slot.  
Any number of Reservations may exist for a given time slot.

Important properties of Reservations:
 - Unique ID (PRIVATE (only sent to requests authorized with the `spectator` role, used for identifying Reservations for confirming, deleting, etc)
 - Optional Location (Country & district for visualization, analytics, etc)
 - Email (PRIVATE (only sent to requests authorized with the `spectator` role), for delivering reminder emails, confirming the Reservation, etc)
 - Year, month, date, and slot number

**`/api/reservations/summary?year=<year>&month=<month>`**
---------------------------------------------------------
**HTTP Method**: `GET`  
**Required Role**: none  
**Response Body**:
```json
"data": [
    {
        "count": 10,
        "locations": [
            {
                "country": "USA",
                "district": "WA"
            },
            ...
        ]
    },
    ...
]
```
**Response Codes**:
 - `404 Not Found` if the given `year` and `month` parameters do not specify a valid month.

**`/api/reservations/day?year=<year>&month=<month>&day=<day>`**
---------------------------------------------------------------
**HTTP Method**: `GET`  
**Required Role**: none  
**Response Body**:
```json
"data": [
    [
        {
            "country": "USA",
            "district": "WA"
        },
        ...
    ],
    ...
]
```
**Response Codes**:
 - `404 Not Found` if the given `year`, `month`, and `day` parameters do not specify a valid date.

**`/api/reservations/create`**
------------------------------
**HTTP Method**: `POST`  
**Required Role**: none  
**Request Body**:
```json
{
    "email": "test@example.com",
    "country": "USA", // Uppercase ISO 3166-1 alpha-3 country code
    "district": "WA",
    "slots": [
        {
            "year": 2019,
            "monthIndex": 0,
            "dayIndex": 0,
            "slotIndex": 30
        },
        ...
    ]
}
```
**Response Body**:
```json
"data": null
```
**Response Codes**:
 - `400 Bad Request` if the values of `country` or `district` are not valid, or if the `year`, `monthIndex`, `dayIndex`, and `slotIndex` do not specify a valid slot in the future, or if there is already a pending confirmation for the given email address, or if any of the requested slots are already reserved by the given email address.

**`/api/reservations/confirm`**
-------------------------------
**HTTP Method**: `POST`  
**Required Role**: none  
**Request Body**:
```json
{
    "confirmationID": "..."
}
```
**Response Body**:
```json
"data": {
    "slots": [
        {
            "year": 2019,
            "monthIndex": 0,
            "dayIndex": 0,
            "slotIndex": 30
        },
        ...
    ]
}
```
**Response Codes**:
 - `404 Not Found` if no pending confirmation with the given `confirmationID` exists.

Locations
---------

To keep clients & the server in sync with country and district codes, clients can obtain their country & district codes from the server.

**`/api/locations/list`**
-------------------------
**HTTP Method**: `GET`  
**Required Role**: none  
**Response Body**:
```json
{
    "USA": {
        "name": "United States of America",
        "districts": {
            "WA": {
                "name": "Washington"
            },
            "OR": {
                "name": "Oregon"
            },
            ...
        }
    },
    ...
}
```

Endorsements
------------

Endorsements are suggested organizations for visitors to donate to.

Important properties of Endorsements:
 - URL of the organization's homepage
 - URL of the organization's official donation page
 - Summary of the organization's mission

**`/api/endorsements/list`**
----------------------------
**HTTP Method**: `GET`  
**Required Role**: none  
**Response Body**:
```json
"data": {
    "currentIndex": 10,
    "endorsements": [
        {
            "homepageURL": "https://www.example.com",
            "donateURL": "https://www.example.com/donate/",
            "summary": "Example.com is doing big things in the Example Website industry, and turns around over 90% of its donations into improving its expansive example infrastructure."
        },
        ...
    ]
}
```

**`/api/endorsements/current`**
-------------------------------
**HTTP Method**: `GET`  
**Required Role**: none  
**Response Body**:
```json
"data": {
    "homepageURL": "https://www.example.com",
    "donateURL": "https://www.example.com/donate/",
    "summary": "Example.com is doing big things in the Example Website industry, and turns around over 90% of its donations into improving its expansive example infrastructure."
}
```
**Response Codes**:
 - `404 Not Found` if there are no endorsements.

**`/api/endorsements/update`**
------------------------------
**HTTP Method**: `POST`  
**Required Role**: `admin`  
**Request Body**:
```json
{
    "currentIndex": 10,
    "endorsements": [
        {
            "homepageURL": "https://www.example.com",
            "donateURL": "https://www.example.com/donate/",
            "summary": "Example.com is doing big things in the Example Website industry, and turns around over 90% of its donations into improving its expansive example infrastructure."
        },
        ...
    ]
}
```
**Response Body**:
```json
"data": null
```
**Response Codes**:
 - `400 Bad Request` if the value of `currentIndex` is greater than or equal to the number of elements in `endorsements` and `endorsements` is not empty, or if `endorsements` is empty and `currentIndex` is not equal to 0.