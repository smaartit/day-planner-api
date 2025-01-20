using Amazon;
using Microsoft.AspNetCore.Mvc;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace DayPlannerApi.Controllers;

[Route("api/[controller]")]
[Produces("application/json")]
[ApiController]
public class TaskController : ControllerBase
{
    private readonly AmazonDynamoDBClient _dynamoDbClient;
    private readonly string _tableName = "DayPlannerTasks";

    public TaskController()
    {
        var region = RegionEndpoint.USEast1;
        _dynamoDbClient = new AmazonDynamoDBClient(region);
    }

    // GET api/task
    [HttpGet("all")]
    public async Task<IActionResult> GetAllTasks(int userId)
    {
        var tasks = new List<Task>();
        var lastEvaluatedKey = new Dictionary<string, AttributeValue>();

        try
        {
            var queryRequest = new QueryRequest
            {
                TableName = "DayPlannerTasks",
                IndexName = "userIdIndex",
                KeyConditionExpression = "userId = :userId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":userId", new AttributeValue { N = userId.ToString() } }
                },
                ExclusiveStartKey = lastEvaluatedKey
            };

            do
            {
                var queryResponse = await _dynamoDbClient.QueryAsync(queryRequest);
                tasks.AddRange(queryResponse.Items.Select(item => new Task
                {
                    Id = int.Parse(item["id"].N),
                    UserId = int.Parse(item["userId"].N),
                    Description = item["description"].S,
                    Color = item.ContainsKey("color") ? item["color"].S : null,
                    AllDay = item["allDay"].N == "1",
                    Start = DateTime.TryParse(item["start"].S, out DateTime start) ? (DateTime?)start : null,
                    End = DateTime.TryParse(item["end"].S, out DateTime end) ? (DateTime?)end : null
                }));

                lastEvaluatedKey = queryResponse.LastEvaluatedKey;

            } while (lastEvaluatedKey.Count > 0);

            return Ok(tasks);
        }
        catch (ProvisionedThroughputExceededException ex)
        {
            return StatusCode(503, $"Throughput exceeded: {ex.Message}");
        }
        catch (AmazonDynamoDBException ex)
        {
            return StatusCode(500, $"DynamoDB error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    // GET api/task/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskById(int id)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { N = id.ToString() } }
            }
        };

        try
        {
            var response = await _dynamoDbClient.GetItemAsync(request);

            if (response.Item == null || response.Item.Count == 0)
                return NotFound("Task not found");

            var taskModel = new Task
            {
                Id = int.Parse(response.Item["id"].N),
                UserId = int.Parse(response.Item["userId"].N),
                Description = response.Item["description"].S,
                Color = response.Item.ContainsKey("color") ? response.Item["color"].S : null,
                AllDay = response.Item["allDay"].N == "1",
                Start = DateTime.TryParse(response.Item["start"].S, out DateTime start) ? (DateTime?)start : null,
                End = DateTime.TryParse(response.Item["end"].S, out DateTime end) ? (DateTime?)end : null
            };

            return Ok(taskModel);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    // POST api/task
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] Task taskModel)
    {
        if (taskModel == null)
        {
            return BadRequest("Invalid task data.");
        }

        // Prepare the request for inserting into DynamoDB
        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { N = taskModel.Id.ToString() } },
                { "userId", new AttributeValue { N = taskModel.UserId.ToString() } },
                { "description", new AttributeValue { S = taskModel.Description } },
                { "color", new AttributeValue { S = taskModel.Color ?? "" } }, // Optional field
                { "allDay", new AttributeValue { N = taskModel.AllDay ? "1" : "0" } },
                { "start", new AttributeValue { S = taskModel.Start?.ToString("o") ?? "" } }, // ISO 8601 format
                { "end", new AttributeValue { S = taskModel.End?.ToString("o") ?? "" } } // ISO 8601 format
            }
        };

        try
        {
            await _dynamoDbClient.PutItemAsync(request);
            return Ok("Task inserted");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    // DELETE api/task/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        try
        {
            var request = new DeleteItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "id", new AttributeValue { N = id.ToString() } }
                }
            };

            var response = await _dynamoDbClient.DeleteItemAsync(request);
            if (response.Attributes == null || response.Attributes.Count == 0)
            {
                return NotFound($"Task with ID {id} not found.");
            }

            return Ok(new
            {
                Message = $"Task with ID {id} deleted successfully.",
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
}