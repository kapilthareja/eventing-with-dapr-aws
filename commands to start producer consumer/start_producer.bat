cd C:\Users\admin\Desktop\POC\eventing-with-dapr-aws\producer

dapr run --app-id producerapp --dapr-http-port 3500 --resources-path ..\components -- dotnet run
