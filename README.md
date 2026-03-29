# Market Price API Documentation

## Overview

**Market Price API** is a RESTful service designed to provide access to financial information, including assets, real-time prices from various providers, and historical price data. The project is implemented using **ASP.NET** with **.NET SDK 10.0** and leverages the [Fintacharts API](https://fintatech.com/) for fetching both REST and real-time price updates. **MongoDB** is used for storing and caching asset data to ensure fast access.

## Quick Start

Prerequisites: Command Line Interface (CLI), Git , Docker
### Steps

1. **Clone the repository**:

```bash
git clone https://github.com/QDestTM/market-price-api.git
cd market-price-api
```

2. **Download MongoDB snapshot**:

From the [Releases tab](https://github.com/qdesttm/market-price-api/releases), download either `mongodb_empty.zip` or `mongodb_empty.rar`. Extract the contents into the project root. A folder named `mongodb` should appear. This snapshot contains pre-configured collections and users for fast setup. For manual setup, see the "Manual MongoDB Setup" section.

3. **Configure `.env` file**:

Create a `.env` file, copy the contents from `.env.example` into it, and then update the following variables:

```env
FINTACHARTS_USERNAME="your_username"
FINTACHARTS_PASSWORD="your_password"

MONGODB_USERNAME="api_service"
MONGODB_PASSWORD="api-yvt9-ap"

FORCE_SWAGGER=true
```

> ⚠️ MongoDB credentials must match the snapshot or your custom setup.<br/>
> ⚠️ Obtain test credentials for **Fintacharts API** independently.<br/>
> ℹ `FORCE_SWAGGER=true` enables Swagger for easy API testing.<br/>

4. **Build the Docker image**:

```bash
docker build -t market-price-api .
```

5. **Start services using Docker Compose**:

```bash
docker-compose up
```

> ⚠️ First launch may take time if MongoDB images are not cached; they will be downloaded automatically.

| Resource       | URL                               |
| -------------- | --------------------------------- |
| Access the API | `https://localhost:23000`         |
| Swagger UI     | `https://localhost:23000/swagger` |

> ⚠️ On the first run with an empty database, some resources may be temporarily unavailable while the server fetches and stores asset data via the API. Please allow a short wait.

## Manual MongoDB Setup

> ℹ Requires MongoDB shell access. Ensure the server is started with `--noauth` for unauthenticated access.

### Create database and collections:

```js
use market

db.createCollection("assets")
db.createCollection("system")
```

### Create API user:

```js
use admin

db.createUser({
    user: "<MONGODB_USERNAME>",
    pwd: "<MONGODB_PASSWORD>",
    roles: [
        { role: "readWrite", db: "market" }
    ]
})
```

Replace `<MONGODB_USERNAME>` and `<MONGODB_PASSWORD>` with the corresponding values from your `.env` file. If you are setting up MongoDB manually, these variables should match what you define in `.env`.

## Using Visual Studio Code

The project includes full Visual Studio Code configuration for debugging:

| Task                     | Description                             |
| ------------------------ | --------------------------------------- |
| `build-market-price-api` | Build the project                       |
| `start-mongo-db`         | Launch MongoDB (`--auth` or `--noauth`) |

Press **F5** to start debugging after MongoDB is running locally or via Docker. The pre-build task `build-market-price-api` ensures the project is compiled before launch.

## Swagger Activation in Production

The `.env` variable `FORCE_SWAGGER` allows forcing Swagger UI activation:

- **Development Mode**: Swagger is enabled by default.
- **Production Mode**: Swagger is disabled by default, unless `FORCE_SWAGGER=true`.

> ℹ Swagger UI is accessible at `/swagger`.

## API Endpoints

### Assets

**Get single asset by ID**:

`➡ GET /api/v1/assets/{asset_id:guid}`

Sample response:

```json
{
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "symbol": "string",
    "kind": "string",
    "tickSize": 0,
    "description": "string",
    "currency": "string",
    "baseCurrency": "string",
    "contractSize": 0
  }
}
```

**Get assets list with pagination and filtering**:

`➡ GET /api/v1/assets`

| Parameter | Type     | Description        |
| --------- | -------- | ------------------ |
| `kind`    | `string` | Asset type         |
| `symbol`  | `string` | Currency or symbol |
| `size`    | `int`    | Page size          |
| `page`    | `int`    | Page number        |

Sample response:

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "symbol": "string",
      "kind": "string",
      "tickSize": 0,
      "description": "string",
      "currency": "string",
      "baseCurrency": "string",
      "contractSize": 0
    }
  ],
  "pages": {
    "page": 0,
    "pages": 0,
    "items": 0
  }
}
```

### Prices

**Get real-time price**:

`➡ GET /api/v1/prices/{provider:string}/{asset_id:guid}`

> ⚠️ This endpoint may be unstable and can occasionally produce unexpected errors.<br/>
> ℹ Returns `404` if data is not available or has not yet been updated. In most cases, a repeated request will return the data.<br/>

Sample response:

```json
{
  "data": {
    "instrumentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "provider": "string",
    "ask": { "price": 0, "volume": 0, "timestamp": "2026-03-29T21:59:56.496Z" },
    "bid": { "price": 0, "volume": 0, "timestamp": "2026-03-29T21:59:56.496Z" },
    "last": {
      "price": 0,
      "volume": 0,
      "timestamp": "2026-03-29T21:59:56.496Z",
      "changePct": 0,
      "change": 0
    },
    "last_updated": "2026-03-29T21:59:56.496Z"
  }
}
```

**Get historical prices**:

`➡ GET /api/v1/prices/history`

| Parameter       | Type       | Description                                     |
| --------------- | ---------- | ----------------------------------------------- |
| `instrument_id` | `Guid`     | Asset ID                                        |
| `provider`      | `string`   | Data provider                                   |
| `interval`      | `int`      | Step between sampled data points                |
| `periodicity`   | `string`   | Frequency of sampling (`minute`, `hour`, `day`) |
| `start_date`    | `DateTime` | Start date (`yyyy-MM-dd`)                       |
| `end_date`      | `DateTime` | End date (`yyyy-MM-dd`)                         |

Sample response:

```json
{
  "data": [
    {
      "time": "2026-03-29T22:03:52.538Z",
      "open": 0,
      "high": 0,
      "low": 0,
      "close": 0,
      "volume": 0
    }
  ]
}
```