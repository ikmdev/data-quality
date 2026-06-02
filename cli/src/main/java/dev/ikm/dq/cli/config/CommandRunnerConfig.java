package dev.ikm.dq.cli.config;

import dev.ikm.dq.cli.evaluate.EvaluateCommand;
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
	                                    EvaluateCommand evaluateCommand) {
		return _ -> {
			int exitCode = new CommandLine(rootCommand)
					.addSubcommand(evaluateCommand)
					.execute(appArgs.getSourceArgs());
			if (exitCode != 0) {
				System.exit(exitCode);
			}
		};
	}
}
