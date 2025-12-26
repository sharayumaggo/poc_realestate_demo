# 04 - DevOps & CI/CD

## CI/CD Pipeline Overview

### GitHub Actions Workflow
```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal

  build-backend:
    needs: test
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Login to Amazon ECR
        uses: aws-actions/amazon-ecr-login@v1
      - name: Build and push Docker image
        run: |
          docker build -t real-estate-api ./RealEstateApi
          docker tag real-estate-api:latest ${{ secrets.AWS_ACCOUNT_ID }}.dkr.ecr.${{ secrets.AWS_REGION }}.amazonaws.com/real-estate-api:${{ github.sha }}
          docker push ${{ secrets.AWS_ACCOUNT_ID }}.dkr.ecr.${{ secrets.AWS_REGION }}.amazonaws.com/real-estate-api:${{ github.sha }}

  build-frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '18'
          cache: 'npm'
          cache-dependency-path: RealEstateFrontend/package-lock.json
      - name: Install dependencies
        run: npm ci
        working-directory: RealEstateFrontend
      - name: Build
        run: npm run build
        working-directory: RealEstateFrontend
      - name: Deploy to S3
        uses: jakejarvis/s3-sync-action@v0.5.1
        with:
          args: --delete
        env:
          AWS_S3_BUCKET: ${{ secrets.AWS_S3_BUCKET }}
          AWS_ACCESS_KEY_ID: ${{ secrets.AWS_ACCESS_KEY_ID }}
          AWS_SECRET_ACCESS_KEY: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          SOURCE_DIR: RealEstateFrontend/build

  deploy:
    needs: [build-backend, build-frontend]
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Deploy to ECS
        uses: aws-actions/amazon-ecs-deploy-task-definition@v1
        with:
          task-definition: task-definition.json
          service: real-estate-api-service
          cluster: real-estate-cluster
          wait-for-service-stability: true
```

## Infrastructure as Code

### Terraform State Management
```hcl
terraform {
  backend "s3" {
    bucket = "real-estate-terraform-state"
    key    = "terraform.tfstate"
    region = "us-east-1"
  }
}
```

### Environment-Specific Configurations
```hcl
# environments/dev/terraform.tfvars
aws_region  = "us-east-1"
environment = "dev"
db_instance_class = "db.t3.micro"

# environments/prod/terraform.tfvars
aws_region  = "us-east-1"
environment = "prod"
db_instance_class = "db.t3.medium"
```

## Monitoring & Observability

### CloudWatch Setup
```hcl
resource "aws_cloudwatch_log_group" "api" {
  name              = "/ecs/real-estate-api"
  retention_in_days = 30

  tags = {
    Environment = var.environment
  }
}

resource "aws_cloudwatch_metric_alarm" "api_cpu" {
  alarm_name          = "real-estate-api-cpu-utilization"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = "2"
  metric_name         = "CPUUtilization"
  namespace           = "AWS/ECS"
  period              = "300"
  statistic           = "Average"
  threshold           = "80"
  alarm_description   = "This metric monitors ECS CPU utilization"

  dimensions = {
    ClusterName = aws_ecs_cluster.main.name
    ServiceName = aws_ecs_service.api.name
  }
}
```

### Application Monitoring
```csharp
// Startup.cs
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddUrlGroup(new Uri(builder.Configuration["ExternalService:HealthCheck"]));
```

### Log Aggregation
```json
{
  "timestamp": "2024-01-01T12:00:00Z",
  "level": "Information",
  "message": "Property created successfully",
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "propertyId": "456e7890-e89b-12d3-a456-426614174001",
  "correlationId": "abc-123-def-456"
}
```

## Security

### Secrets Management
```hcl
resource "aws_secretsmanager_secret" "db_password" {
  name = "real-estate/db-password"
}

resource "aws_secretsmanager_secret_version" "db_password" {
  secret_id     = aws_secretsmanager_secret.db_password.id
  secret_string = var.db_password
}
```

### IAM Policies
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "ecr:GetDownloadUrlForLayer",
        "ecr:BatchGetImage",
        "ecr:GetAuthorizationToken"
      ],
      "Resource": "*"
    },
    {
      "Effect": "Allow",
      "Action": [
        "s3:GetObject",
        "s3:PutObject",
        "s3:DeleteObject"
      ],
      "Resource": "${aws_s3_bucket.frontend.arn}/*"
    }
  ]
}
```

### Security Scanning
```yaml
- name: Security Scan
  uses: securecodewarrior/github-actions-gosec@master
  with:
    args: './...'

- name: Dependency Check
  uses: dependency-check/Dependency-Check_Action@main
  with:
    project: 'Real Estate API'
    path: '.'
    format: 'ALL'
```

## Deployment Strategy

### Blue-Green Deployment
```hcl
resource "aws_lb_listener" "api" {
  # Production listener points to blue target group
}

resource "aws_lb_listener" "api_green" {
  # Green listener for testing new deployments
  port = 8080
}
```

### Rolling Updates
```hcl
resource "aws_ecs_service" "api" {
  deployment_minimum_healthy_percent = 50
  deployment_maximum_percent         = 200

  deployment_controller {
    type = "ECS"
  }
}
```

## Backup & Disaster Recovery

### Database Backup
```hcl
resource "aws_backup_plan" "database" {
  name = "real-estate-database-backup"

  rule {
    rule_name         = "daily-backup"
    target_vault_name = aws_backup_vault.main.name
    schedule          = "cron(0 5 ? * * *)"

    lifecycle {
      delete_after = 30
    }
  }
}
```

### Multi-Region Deployment (Future)
```hcl
# Primary region (us-east-1)
# Secondary region (us-west-2) with Route 53 failover
resource "aws_route53_record" "api" {
  set_identifier = "primary"
  failover_routing_policy {
    type = "PRIMARY"
  }
}

resource "aws_route53_record" "api_failover" {
  set_identifier = "secondary"
  failover_routing_policy {
    type = "SECONDARY"
  }
}
```

## Cost Optimization

### Auto Scaling
```hcl
resource "aws_appautoscaling_target" "ecs_target" {
  max_capacity       = 10
  min_capacity       = 1
  resource_id        = "service/${aws_ecs_cluster.main.name}/${aws_ecs_service.api.name}"
  scalable_dimension = "ecs:service:DesiredCount"
  service_namespace  = "ecs"
}

resource "aws_appautoscaling_policy" "ecs_cpu" {
  name               = "cpu-autoscaling"
  policy_type        = "TargetTrackingScaling"
  resource_id        = aws_appautoscaling_target.ecs_target.resource_id
  scalable_dimension = aws_appautoscaling_target.ecs_target.scalable_dimension
  service_namespace  = aws_appautoscaling_target.ecs_target.service_namespace

  target_tracking_scaling_policy_configuration {
    predefined_metric_specification {
      predefined_metric_type = "ECSServiceAverageCPUUtilization"
    }
    target_value = 70.0
  }
}
```

### Cost Monitoring
```hcl
resource "aws_budgets_budget" "monthly" {
  name         = "real-estate-monthly-budget"
  budget_type  = "COST"
  limit_amount = "100"
  limit_unit   = "USD"
  time_unit    = "MONTHLY"

  notification {
    comparison_operator        = "GREATER_THAN"
    threshold                  = 80
    threshold_type            = "PERCENTAGE"
    notification_type         = "ACTUAL"
    subscriber_email_addresses = ["admin@realestate.com"]
  }
}
```

## Performance Optimization

### CDN Configuration
```hcl
resource "aws_cloudfront_distribution" "frontend" {
  # ... existing config

  # Cache behaviors for API
  ordered_cache_behavior {
    path_pattern     = "/api/*"
    allowed_methods  = ["GET", "HEAD", "OPTIONS", "PUT", "POST", "PATCH", "DELETE"]
    cached_methods   = ["GET", "HEAD"]
    target_origin_id = "api-origin"

    forwarded_values {
      query_string = true
      headers      = ["Authorization"]
      cookies {
        forward = "all"
      }
    }

    min_ttl     = 0
    default_ttl = 0
    max_ttl     = 0

    lambda_function_association {
      event_type   = "viewer-request"
      lambda_arn   = aws_lambda_function.api_auth.qualified_arn
    }
  }
}
```

### Database Performance
```sql
-- Performance indexes
CREATE INDEX idx_properties_location ON properties USING GIST (location);
CREATE INDEX idx_properties_price ON properties (price);
CREATE INDEX idx_properties_status ON properties (status);
CREATE INDEX idx_favorites_user_property ON favorites (user_id, property_id);
```

## Compliance & Governance

### Tagging Strategy
```hcl
locals {
  common_tags = {
    Project     = "Real Estate Marketplace"
    Environment = var.environment
    Owner       = "DevOps Team"
    CostCenter  = "Engineering"
    ManagedBy   = "Terraform"
  }
}
```

### Resource Naming Convention
```
{project}-{component}-{environment}-{region}
real-estate-api-dev-us-east-1
real-estate-db-prod-us-east-1
```

## Incident Response

### Alert Configuration
```hcl
resource "aws_sns_topic" "alerts" {
  name = "real-estate-alerts"
}

resource "aws_cloudwatch_metric_alarm" "api_5xx" {
  alarm_name          = "real-estate-api-5xx-errors"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = "2"
  metric_name         = "HTTPCode_Target_5XX_Count"
  namespace           = "AWS/ApplicationELB"
  period              = "300"
  statistic           = "Sum"
  threshold           = "10"

  alarm_actions = [aws_sns_topic.alerts.arn]
}
```

### Runbooks
- Database failover procedures
- Application deployment rollback
- Security incident response
- Performance degradation troubleshooting

## Future Enhancements

### Advanced Monitoring
- Distributed tracing with X-Ray
- Real user monitoring (RUM)
- Log analytics with CloudWatch Insights
- Custom dashboards

### Automation
- Infrastructure testing with Terratest
- Configuration management with Ansible
- GitOps with ArgoCD
- Policy as Code with OPA

### Security Enhancements
- AWS WAF for API protection
- AWS Shield for DDoS protection
- AWS Config for compliance monitoring
- AWS GuardDuty for threat detection