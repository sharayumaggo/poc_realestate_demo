output "api_endpoint" {
  description = "API endpoint URL"
  value       = aws_lb.main.dns_name
}

output "frontend_url" {
  description = "Frontend CloudFront URL"
  value       = aws_cloudfront_distribution.frontend.domain_name
}

output "database_endpoint" {
  description = "Database endpoint"
  value       = aws_db_instance.main.address
}

output "ecr_repository_url" {
  description = "ECR repository URL for API"
  value       = aws_ecr_repository.api.repository_url
}

output "s3_bucket_name" {
  description = "S3 bucket name for frontend"
  value       = aws_s3_bucket.frontend.bucket
}

output "vpc_id" {
  description = "VPC ID"
  value       = aws_vpc.main.id
}

output "ecs_cluster_name" {
  description = "ECS cluster name"
  value       = aws_ecs_cluster.main.name
}

output "ecs_service_name" {
  description = "ECS service name"
  value       = aws_ecs_service.api.name
}