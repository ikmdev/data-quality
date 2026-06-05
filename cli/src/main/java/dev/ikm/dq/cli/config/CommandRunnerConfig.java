package dev.ikm.dq.cli.config;

import dev.ikm.dq.cli.pipeline.PipelineCommand;
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
										PipelineCommand pipelineCommand) {
		return _ -> {
			int exitCode = new CommandLine(rootCommand)
					.addSubcommand(pipelineCommand)
					.execute(appArgs.getSourceArgs());
			System.exit(exitCode);
		};
	}
}
