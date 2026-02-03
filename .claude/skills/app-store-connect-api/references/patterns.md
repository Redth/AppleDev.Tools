# API Request/Response Patterns

## JSON:API Structure

App Store Connect API follows JSON:API specification. All requests and responses use this structure:

### Response Format
```json
{
  "data": { ... } | [ ... ],     // Single resource or array
  "included": [ ... ],           // Optional: related resources
  "links": { "self": "..." },    // Pagination links
  "meta": { "paging": {...} }    // Pagination metadata
}
```

### Resource Object
```json
{
  "type": "bundleIds",           // Resource type (always plural)
  "id": "ABC123",                // Resource ID
  "attributes": { ... },         // Resource data
  "relationships": { ... },      // Related resources
  "links": { "self": "..." }
}
```

### Request Format (Create/Update)
```json
{
  "data": {
    "type": "bundleIds",
    "attributes": { ... },
    "relationships": { ... }     // Optional: link to related resources
  }
}
```

## Common Patterns

### List with Filtering
```
GET /v1/bundleIds?filter[platform]=IOS&filter[identifier]=com.example.*
```

Filter operators:
- Exact match: `filter[field]=value`
- Multiple values: `filter[field]=value1,value2`

### List with Sorting
```
GET /v1/bundleIds?sort=name       # Ascending
GET /v1/bundleIds?sort=-name      # Descending
```

### Pagination
```
GET /v1/bundleIds?limit=50
```
Response includes `links.next` for next page.

### Include Related Resources
```
GET /v1/bundleIds?include=bundleIdCapabilities,profiles
```
Related resources appear in `included` array.

### Field Selection
```
GET /v1/bundleIds?fields[bundleIds]=name,identifier,platform
```

## Relationship Endpoints

Many resources have sub-endpoints for relationships:

```
GET  /v1/bundleIds/{id}/bundleIdCapabilities    # List capabilities
GET  /v1/bundleIds/{id}/profiles                # List profiles
GET  /v1/bundleIds/{id}/relationships/profiles  # Get relationship links only
```

## Error Response Format
```json
{
  "errors": [
    {
      "id": "...",
      "status": "400",
      "code": "PARAMETER_ERROR.INVALID",
      "title": "A parameter is invalid",
      "detail": "The filter 'badFilter' is not valid for this request"
    }
  ]
}
```

Common error codes:
- `PARAMETER_ERROR.INVALID` - Invalid query parameter
- `PARAMETER_ERROR.MISSING` - Required parameter missing
- `NOT_FOUND` - Resource doesn't exist
- `FORBIDDEN` - Insufficient permissions
- `CONFLICT` - Resource state conflict
- `AUTHENTICATION_ERROR` - JWT token issues

## Rate Limiting

Responses include headers:
- `X-Rate-Limit-Limit` - Requests allowed per hour
- `X-Rate-Limit-Remaining` - Requests remaining

When exceeded, returns HTTP 429 with `Retry-After` header.
