cd C:\Users\admin\Desktop\POC\eventing-with-dapr-aws\consumer

dapr run --app-id consumerapp --app-port 5237 --dapr-http-port 3501 --resources-path ..\components --log-level debug -- dotnet run
