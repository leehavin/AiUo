import React from 'react';
import clsx from 'clsx';
import styles from './styles.module.css';

const FeatureList = [
  {
    title: '🚀 高性能架构',
    Svg: require('@site/static/img/performance.svg').default,
    description: (
      <>
        基于.NET 8.0构建，采用现代化架构设计。内置高效缓存机制、
        异步编程模型和AOT编译优化，提供卓越的运行性能。
      </>
    ),
  },
  {
    title: '🔧 模块化设计',
    Svg: require('@site/static/img/modular.svg').default,
    description: (
      <>
        采用模块化架构，50+核心组件可按需引入。支持插件式扩展，
        让您的应用程序更加灵活和可维护。
      </>
    ),
  },
  {
    title: '🛡️ 企业级安全',
    Svg: require('@site/static/img/security.svg').default,
    description: (
      <>
        内置完善的安全机制，包括JWT认证、权限控制、数据加密等。
        符合企业级安全标准，保障您的应用程序安全可靠。
      </>
    ),
  },
  {
    title: '📊 智能数据访问',
    Svg: require('@site/static/img/database.svg').default,
    description: (
      <>
        支持多种数据库，内置读写分离、分库分表、缓存策略。
        基于SqlSugar ORM，提供强类型查询和高性能数据访问。
      </>
    ),
  },
  {
    title: '☁️ 云原生支持',
    Svg: require('@site/static/img/cloud.svg').default,
    description: (
      <>
        原生支持Docker容器化部署，集成Kubernetes编排。
        支持微服务架构，提供服务发现、负载均衡等云原生特性。
      </>
    ),
  },
  {
    title: '🔄 实时通信',
    Svg: require('@site/static/img/realtime.svg').default,
    description: (
      <>
        内置SignalR实时通信、RabbitMQ消息队列、Redis缓存。
        支持事件驱动架构，构建响应式应用程序。
      </>
    ),
  },
];

function Feature({Svg, title, description}) {
  return (
    <div className={clsx('col col--4', styles.feature)}>
      <div className={styles.featureCard}>
        <div className={styles.featureIcon}>
          <Svg className={styles.featureSvg} role="img" />
        </div>
        <div className={styles.featureContent}>
          <h3 className={styles.featureTitle}>{title}</h3>
          <p className={styles.featureDescription}>{description}</p>
        </div>
      </div>
    </div>
  );
}

export default function HomepageFeatures() {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          <div className="col col--12">
            <div className={styles.sectionHeader}>
              <h2 className={styles.sectionTitle}>为什么选择 AiUo？</h2>
              <p className={styles.sectionSubtitle}>
                AiUo 提供了构建现代化企业级应用程序所需的一切工具和组件
              </p>
            </div>
          </div>
        </div>
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
        
        {/* Additional Features Section */}
        <div className="row" style={{marginTop: '4rem'}}>
          <div className="col col--12">
            <div className={styles.additionalFeatures}>
              <h3 className={styles.additionalTitle}>更多特性</h3>
              <div className={styles.featureGrid}>
                <div className={styles.featureItem}>
                  <div className={styles.featureItemIcon}>📝</div>
                  <div className={styles.featureItemContent}>
                    <h4>丰富的日志记录</h4>
                    <p>集成Serilog，支持多种输出目标</p>
                  </div>
                </div>
                <div className={styles.featureItem}>
                  <div className={styles.featureItemIcon}>🔍</div>
                  <div className={styles.featureItemContent}>
                    <h4>健康检查</h4>
                    <p>内置应用程序健康监控机制</p>
                  </div>
                </div>
                <div className={styles.featureItem}>
                  <div className={styles.featureItemIcon}>⚡</div>
                  <div className={styles.featureItemContent}>
                    <h4>自动映射</h4>
                    <p>集成AutoMapper，简化对象转换</p>
                  </div>
                </div>
                <div className={styles.featureItem}>
                  <div className={styles.featureItemIcon}>🌐</div>
                  <div className={styles.featureItemContent}>
                    <h4>国际化支持</h4>
                    <p>内置多语言和本地化支持</p>
                  </div>
                </div>
                <div className={styles.featureItem}>
                  <div className={styles.featureItemIcon}>📧</div>
                  <div className={styles.featureItemContent}>
                    <h4>邮件服务</h4>
                    <p>集成SMTP邮件发送功能</p>
                  </div>
                </div>
                <div className={styles.featureItem}>
                  <div className={styles.featureItemIcon}>🔐</div>
                  <div className={styles.featureItemContent}>
                    <h4>加密工具</h4>
                    <p>提供多种加密和哈希算法</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}