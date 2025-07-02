import React from 'react';
import clsx from 'clsx';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Layout from '@theme/Layout';
import HomepageFeatures from '@site/src/components/HomepageFeatures';

import styles from './index.module.css';

function HomepageHeader() {
  const {siteConfig} = useDocusaurusContext();
  return (
    <header className={clsx('hero hero--primary', styles.heroBanner)}>
      <div className="container">
        <div className="row">
          <div className="col col--6">
            <h1 className="hero__title animate-fade-in-up">{siteConfig.title}</h1>
            <p className="hero__subtitle animate-fade-in-up">{siteConfig.tagline}</p>
            <div className={styles.buttons}>
              <Link
                className="button button--secondary button--lg"
                to="/docs/intro">
                🚀 快速开始
              </Link>
              <Link
                className="button button--primary button--lg"
                to="/docs/quick-start"
                style={{marginLeft: '1rem'}}>
                📖 查看文档
              </Link>
            </div>
            <div className={styles.stats}>
              <div className={styles.stat}>
                <div className={styles.statNumber}>50+</div>
                <div className={styles.statLabel}>核心组件</div>
              </div>
              <div className={styles.stat}>
                <div className={styles.statNumber}>100K+</div>
                <div className={styles.statLabel}>下载量</div>
              </div>
              <div className={styles.stat}>
                <div className={styles.statNumber}>99.9%</div>
                <div className={styles.statLabel}>可用性</div>
              </div>
            </div>
          </div>
          <div className="col col--6">
            <div className={styles.heroImage}>
              <div className={styles.codeBlock}>
                <div className={styles.codeHeader}>
                  <span className={styles.codeDot}></span>
                  <span className={styles.codeDot}></span>
                  <span className={styles.codeDot}></span>
                  <span className={styles.codeTitle}>Program.cs</span>
                </div>
                <pre className={styles.codeContent}>
{`var builder = WebApplication.CreateBuilder(args);

// 添加 AiUo 服务
builder.Services.AddAiUo(options => {
    options.UseRedis("localhost:6379");
    options.UseMySql(connectionString);
    options.UseRabbitMQ("localhost:5672");
});

var app = builder.Build();
app.Run();`}
                </pre>
              </div>
            </div>
          </div>
        </div>
      </div>
    </header>
  );
}

export default function Home() {
  const {siteConfig} = useDocusaurusContext();
  return (
    <Layout
      title={`${siteConfig.title} - 企业级.NET开发框架`}
      description="AiUo是一个高性能、可扩展的企业级.NET开发框架，提供丰富的组件和工具，助力开发者快速构建现代化应用程序。">
      <HomepageHeader />
      <main>
        <HomepageFeatures />
        
        {/* Trusted By Section */}
        <section className={styles.trustedBy}>
          <div className="container">
            <h2 className="text-center mb-4">受到众多企业信赖</h2>
            <div className="row">
              <div className="col col--12">
                <div className={styles.logoGrid}>
                  <div className={styles.logoItem}>企业A</div>
                  <div className={styles.logoItem}>企业B</div>
                  <div className={styles.logoItem}>企业C</div>
                  <div className={styles.logoItem}>企业D</div>
                  <div className={styles.logoItem}>企业E</div>
                  <div className={styles.logoItem}>企业F</div>
                </div>
              </div>
            </div>
          </div>
        </section>

        {/* Performance Section */}
        <section className={styles.performance}>
          <div className="container">
            <div className="row">
              <div className="col col--6">
                <h2>🚀 卓越性能</h2>
                <p className="text-secondary">
                  基于.NET 8.0构建，充分利用最新的性能优化特性。内置高效的缓存机制和数据库访问策略，
                  支持异步编程模型，提供卓越的并发性能。
                </p>
                <ul>
                  <li>✅ 高性能数据访问层</li>
                  <li>✅ 智能缓存策略</li>
                  <li>✅ 异步编程支持</li>
                  <li>✅ AOT编译优化</li>
                </ul>
              </div>
              <div className="col col--6">
                <div className={styles.performanceChart}>
                  <h4>性能对比</h4>
                  <div className={styles.chartBar}>
                    <div className={styles.barLabel}>AiUo Framework</div>
                    <div className={styles.bar} style={{width: '100%'}}>
                      <span>150K ops/sec</span>
                    </div>
                  </div>
                  <div className={styles.chartBar}>
                    <div className={styles.barLabel}>EF Core</div>
                    <div className={styles.bar} style={{width: '53%'}}>
                      <span>80K ops/sec</span>
                    </div>
                  </div>
                  <div className={styles.chartBar}>
                    <div className={styles.barLabel}>Dapper</div>
                    <div className={styles.bar} style={{width: '80%'}}>
                      <span>120K ops/sec</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </section>

        {/* CTA Section */}
        <section className={styles.cta}>
          <div className="container">
            <div className="row">
              <div className="col col--12 text-center">
                <h2>准备开始使用 AiUo？</h2>
                <p className="text-secondary mb-4">
                  立即开始构建您的下一个企业级应用程序
                </p>
                <div className={styles.ctaButtons}>
                  <Link
                    className="button button--primary button--lg"
                    to="/docs/quick-start">
                    开始使用
                  </Link>
                  <Link
                    className="button button--secondary button--lg"
                    to="https://github.com/aiuo/aiuo"
                    style={{marginLeft: '1rem'}}>
                    查看源码
                  </Link>
                </div>
              </div>
            </div>
          </div>
        </section>
      </main>
    </Layout>
  );
}