
FROM microsoft/dotnet:2.2-aspnetcore-runtime
COPY ./publish /publish
WORKDIR /publish
# set the argument default
ARG ASPNETCORE_ENVIROMENT=production

# use the argument
RUN echo $ASPNETCORE_ENVIROMENT

RUN apt-get update
RUN rm /bin/sh && ln -s /bin/bash /bin/sh
RUN apt-get -qq -y install \
    curl \
    unzip \
    zip
    
RUN curl -s https://get.sdkman.io | bash

RUN chmod a+x "$HOME/.sdkman/bin/sdkman-init.sh"

RUN source "$HOME/.sdkman/bin/sdkman-init.sh"

RUN export SDKMAN_DIR="$HOME/.sdkman"

RUN yes | /bin/bash -l -c 'sdk install kotlin' 

RUN mkdir -p /usr/share/man/man1 && apt-get update && \
    apt-get install -y openjdk-8-jdk

RUN apt-get update && \
    apt-get install ca-certificates-java && \
    apt-get clean && \
    update-ca-certificates -f;
    
EXPOSE 80
ENTRYPOINT ["dotnet", "RSPeer.Compiler.dll"]