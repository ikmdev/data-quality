package dev.ikm.dq.cli.config;

import org.apache.hc.client5.http.config.ConnectionConfig;
import org.apache.hc.client5.http.config.RequestConfig;
import org.apache.hc.client5.http.impl.classic.CloseableHttpClient;
import org.apache.hc.client5.http.impl.classic.HttpClients;
import org.apache.hc.client5.http.impl.io.PoolingHttpClientConnectionManager;
import org.apache.hc.core5.util.TimeValue;
import org.apache.hc.core5.util.Timeout;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.http.client.HttpComponentsClientHttpRequestFactory;
import org.springframework.web.client.RestClient;

@Configuration
public class PiqiClientConfig {


	// 1. Scale these to match or slightly exceed your virtual thread maxConcurrentRequests (300)
	private static final int MAX_TOTAL_CONNECTIONS = 350;
	private static final int MAX_PER_ROUTE = 350;

	private static final int CONNECT_TIMEOUT_MS = 2000;
	private static final int RESPONSE_TIMEOUT_MS = 5000;
	private static final int MAX_IDLE_CONNECTION_KEEPALIVE_MS = 15000;


	@Bean
	public RestClient piqiRestClient(RestClient.Builder builder, @Value("${piqi.api.base-url}") String baseUrl) {

		// 1. Connection Config handles socket-level timeouts
		ConnectionConfig connectionConfig = ConnectionConfig.custom()
				.setConnectTimeout(Timeout.ofMilliseconds(CONNECT_TIMEOUT_MS))
				.setSocketTimeout(Timeout.ofMilliseconds(RESPONSE_TIMEOUT_MS)) // Use setSocketTimeout here
				.setTimeToLive(TimeValue.ofMilliseconds(MAX_IDLE_CONNECTION_KEEPALIVE_MS))
				.build();

		// 2. Set up the connection manager to allocate and track the socket pool
		PoolingHttpClientConnectionManager connectionManager = new PoolingHttpClientConnectionManager();
		connectionManager.setMaxTotal(MAX_TOTAL_CONNECTIONS);
		connectionManager.setDefaultMaxPerRoute(MAX_PER_ROUTE);
		connectionManager.setDefaultConnectionConfig(connectionConfig);

		// 3. Request Config handles request-level and data arrival timeouts
		RequestConfig requestConfig = RequestConfig.custom()
				.setConnectionRequestTimeout(Timeout.ofMilliseconds(CONNECT_TIMEOUT_MS))
				.setResponseTimeout(Timeout.ofMilliseconds(RESPONSE_TIMEOUT_MS)) // setResponseTimeout belongs here
				.build();

		// 4. Build the Apache CloseableHttpClient
		CloseableHttpClient httpClient = HttpClients.custom()
				.setConnectionManager(connectionManager)
				.setDefaultRequestConfig(requestConfig)
				.disableAutomaticRetries()
				.evictIdleConnections(TimeValue.ofMilliseconds(MAX_IDLE_CONNECTION_KEEPALIVE_MS))
				.build();

		// 5. Wrap inside Spring's request factory framework
		HttpComponentsClientHttpRequestFactory requestFactory = new HttpComponentsClientHttpRequestFactory(httpClient);

		// 6. Output the configured RestClient Bean
		return builder
				.requestFactory(requestFactory)
				.baseUrl(baseUrl)
				.defaultHeader("Content-Type", "application/json")
				.build();
	}
}
