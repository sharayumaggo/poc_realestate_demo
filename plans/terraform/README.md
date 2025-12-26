# Terraform Infrastructure for Real Estate Marketplace

This directory contains the Terraform configuration for deploying the Real Estate Marketplace application to AWS.

## Architecture

The infrastructure includes:
- **VPC** with public and private subnets
- **ECS Fargate** for running the .NET API
- **RDS PostgreSQL** for the database
- **Application Load Balancer** for API traffic
- **S3 + CloudFront** for static frontend hosting
- **ECR** for Docker image storage
- **CloudWatch** for logging

## Prerequisites

1. **AWS CLI configured** with appropriate permissions
2. **Terraform installed** (version 1.0+)
3. **Docker** installed for building images

## Deployment Steps

### 1. Initialize Terraform

```bash
terraform init
```

### 2. Plan the deployment

```bash
terraform plan -var="db_password=your_secure_password"
```

### 3. Apply the configuration

```bash
terraform apply -var="db_password=your_secure_password"
```

### 4. Build and push Docker image

After Terraform creates the ECR repository:

```bash
# Build the API Docker image
cd ../RealEstateApi
docker build -t real-estate-api .

# Tag and push to ECR
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin <account>.dkr.ecr.us-east-1.amazonaws.com
docker tag real-estate-api:latest <ecr-repo-url>:latest
docker push <ecr-repo-url>:latest
```

Replace `<account>` and `<ecr-repo-url>` with actual values from Terraform outputs.

### 5. Deploy frontend to S3

```bash
# Build the frontend
cd ../RealEstateFrontend
npm run build

# Sync to S3
aws s3 sync build/ s3://<bucket-name> --delete
```

Replace `<bucket-name>` with the S3 bucket name from Terraform outputs.

## Configuration Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `aws_region` | AWS region | `us-east-1` |
| `db_username` | Database username | `realestate` |
| `db_password` | Database password (required) | - |
| `environment` | Environment name | `dev` |

## Outputs

After deployment, Terraform will output:
- `api_endpoint`: ALB DNS name for API access
- `frontend_url`: CloudFront distribution URL
- `database_endpoint`: RDS endpoint
- `ecr_repository_url`: ECR repository URL
- `s3_bucket_name`: S3 bucket for frontend
- `vpc_id`: VPC ID
- `ecs_cluster_name`: ECS cluster name
- `ecs_service_name`: ECS service name

## Cost Estimation

This configuration uses:
- **ECS Fargate**: ~$0.01/hour for t3.micro equivalent
- **RDS PostgreSQL**: ~$0.02/hour for db.t3.micro
- **CloudFront**: ~$0.01/GB data transfer
- **S3**: ~$0.02/GB storage
- **ALB**: ~$0.02/hour

**Estimated monthly cost**: ~$50-100 for development/light usage.

## Security Notes

- Database is in private subnets with security groups
- S3 bucket has public access blocked, served via CloudFront
- ALB has HTTPS redirect (update listener for production)
- Use AWS Secrets Manager for sensitive configuration in production

## Cleanup

To destroy all resources:

```bash
terraform destroy -var="db_password=your_secure_password"
```

**Warning**: This will delete all data including the database.