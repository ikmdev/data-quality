package dev.ikm.dq.cli.config;

import dev.ikm.dq.cli.evaluate.EvaluateCommand;
import dev.ikm.dq.cli.ingest.IngestCommand;
import org.springframework.boot.ApplicationArguments;
import org.springframework.boot.CommandLineRunner;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import picocli.CommandLine;

@Configuration
public class CommandRunnerConfig {

	@Bean
	CommandLineRunner commandLineRunner(ApplicationArguments appArgs,
	                                    RootCommand rootCommand,
										IngestCommand ingestCommand,
	                                    EvaluateCommand evaluateCommand) {
		return _ -> {
			int exitCode = new CommandLine(rootCommand)
					.addSubcommand(ingestCommand)
					.addSubcommand(evaluateCommand)
					.execute(appArgs.getSourceArgs());
			System.exit(exitCode);
		};
	}
}
