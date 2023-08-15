using System.Diagnostics;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;

namespace TinyAdmin.Web.Controllers;

[Route("/api")]
public class TinyAdminApiController : Controller
{
    [HttpGet("logs/stream")]
    public async Task LogsStream()
    {
        DockerClient client = new DockerClientConfiguration(
                new Uri("unix:///var/run/docker.sock"))
            .CreateClient();

        string targetContainerName = Environment.GetEnvironmentVariable("TARGET_CONTAINER") ??
                                     throw new ArgumentException("No Target Container");

        var containers = await client.Containers.ListContainersAsync(new ContainersListParameters());
        ContainerListResponse? targetContainer = containers
            .First(container => container.Names.Any(name => name.Contains(targetContainerName)));


        Stream? logsStream = await client.Containers.GetContainerLogsAsync(targetContainer.ID,
            new ContainerLogsParameters { Follow = true, ShowStdout = true });
        var logsStreamReader = new StreamReader(logsStream);

        Response.Headers.Add("Content-Type", "text/event-stream");

        while (true)
        {
            string? log = await logsStreamReader.ReadLineAsync();
            await Response.WriteAsync($"data: {log}\r\r");

            await Response.Body.FlushAsync();
        }
    }

    [HttpGet("logs")]
    public async Task<IActionResult> Logs()
    {
        DockerClient client = new DockerClientConfiguration(
                new Uri("unix:///var/run/docker.sock"))
            .CreateClient();

        string targetContainerName = Environment.GetEnvironmentVariable("TARGET_CONTAINER") ??
                                     throw new ArgumentException("No Target Container");

        var containers = await client.Containers.ListContainersAsync(new ContainersListParameters());
        ContainerListResponse? targetContainer = containers
            .First(container => container.Names.Any(name => name.Contains(targetContainerName)));


        Stream? logsStream = await client.Containers.GetContainerLogsAsync(targetContainer.ID,
            new ContainerLogsParameters { Follow = false, ShowStdout = true });
        var logsStreamReader = new StreamReader(logsStream);
        return Ok(await logsStreamReader.ReadToEndAsync());
    }

    [HttpPost("restart")]
    public async Task<IActionResult> Restart()
    {
        DockerClient client = new DockerClientConfiguration(
                new Uri("unix:///var/run/docker.sock"))
            .CreateClient();

        string targetContainerName = Environment.GetEnvironmentVariable("TARGET_CONTAINER") ??
                                     throw new ArgumentException("No Target Container");

        var containers = await client.Containers.ListContainersAsync(new ContainersListParameters());
        ContainerListResponse? targetContainer = containers
            .First(container => container.Names.Any(name => name.Contains(targetContainerName)));
        await client.Containers.RestartContainerAsync(targetContainer.ID, new ContainerRestartParameters());
        return Ok();
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update()
    {
        string composeFileLocation = Environment.GetEnvironmentVariable("COMPOSE_FILE_LOCATION") ??
                                     throw new ArgumentException("No COMPOSE_FILE_LOCATION");
        string targetContainerName = Environment.GetEnvironmentVariable("TARGET_CONTAINER") ??
                                     throw new ArgumentException("No TARGET_CONTAINER");

        DockerClient client = new DockerClientConfiguration(
                new Uri("unix:///var/run/docker.sock"))
            .CreateClient();

        var containers = await client.Containers.ListContainersAsync(new ContainersListParameters());
        ContainerListResponse? targetContainer = containers
            .First(container => container.Names.Any(name => name.Contains(targetContainerName)));

        string dockerImage = targetContainer.Image.Split(":").First();
        
        Process pullProcess = Process.Start(new ProcessStartInfo("docker")
        {
            WorkingDirectory = composeFileLocation,
            Arguments = $"pull {dockerImage}:latest"
        }) ?? throw new Exception("Can't start pull");
        pullProcess.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
        await pullProcess.WaitForExitAsync();

        Process recomposeProcess = Process.Start(new ProcessStartInfo("docker")
        {
            WorkingDirectory = composeFileLocation,
            Arguments = $"compose up -d {targetContainerName}"
        }) ?? throw new Exception("Can't start update");
        recomposeProcess.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
        await recomposeProcess.WaitForExitAsync();
        return Ok();
    }
}